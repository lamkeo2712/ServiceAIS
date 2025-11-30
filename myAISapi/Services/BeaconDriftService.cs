using myAISapi.Data;
using myAISapi.Helpers;
using myAISapi.Models;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using myAISapi.Encoder;

namespace myAISapi.Services
{
	public class BeaconDriftService
	{
		private readonly AppDBContext _context;
		private readonly IAlertService _alert;

		public BeaconDriftService(AppDBContext context, IAlertService alert)
		{
			_context = context;
			_alert = alert;
		}
		public async Task ScanAllBeaconsAsync()
		{
			var thamSo = "{\"AidType\":\"1\"}";
			var raw = await _context.ExecuteProcedureAsync(
				"Proc_DM_Tau_Search2",
				thamSo,
				""
			);

			var json = JsonSerializer.Serialize(raw);
			using var doc = JsonDocument.Parse(json);
			if (!doc.RootElement.TryGetProperty("DM_Tau", out var dmTauElement) ||
				dmTauElement.ValueKind != JsonValueKind.Array)
			{
				return;
			}

			var options = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				NumberHandling = JsonNumberHandling.AllowReadingFromString
			};

			var beacons = JsonSerializer.Deserialize<List<BeaconCurrentDto>>(dmTauElement.GetRawText(), options)
						  ?? new List<BeaconCurrentDto>();

			foreach (var b in beacons)
			{
				await CheckBeaconDriftAsync(b);
			}
		}

		private async Task CheckBeaconDriftAsync(BeaconCurrentDto current)
		{
			if (!current.Latitude.HasValue || !current.Longitude.HasValue)
				return;

			var lat = current.Latitude.Value;
			var lon = current.Longitude.Value;

			if (!double.IsFinite(lat) || !double.IsFinite(lon))
				return;

			var beaconRef = await _context.DM_BeaconRef
				.FirstOrDefaultAsync(x => x.MMSI == current.MMSI);

			if (beaconRef == null)
			{
				beaconRef = new BeaconRef
				{
					MMSI = current.MMSI,
					BeaconName = current.VesselName,
					RefLat = lat,
					RefLon = lon,
					DriftThresholdMeters = 50,
					IsDrifting = false,
					CreatedAt = DateTime.UtcNow
				};
				_context.DM_BeaconRef.Add(beaconRef);
				await _context.SaveChangesAsync();
				return;
			}

			var distance = GeoHelper.DistanceMeters(
				beaconRef.RefLat,
				beaconRef.RefLon,
				lat,
				lon
			);

			var threshold = beaconRef.DriftThresholdMeters <= 0
				? 50
				: beaconRef.DriftThresholdMeters;

			if (distance > threshold )
			{
				beaconRef.IsDrifting = true;
				beaconRef.LastAlertAt = DateTime.UtcNow;
				beaconRef.UpdatedAt = DateTime.UtcNow;

				await _context.SaveChangesAsync();

				string ais21 = AisAtonEncoder.EncodeType21(
					mmsi: current.MMSI,
					latitudeDeg: beaconRef.RefLat,
					longitudeDeg: beaconRef.RefLon,
					name: beaconRef.BeaconName,
					atonType: current.AidTypeID,
					isVirtual: true
				);

				var alertPayload = new
				{
					Type = "BeaconDrift",
					MMSI = current.MMSI,
					BeaconName = beaconRef.BeaconName ?? current.VesselName,
					RefLat = beaconRef.RefLat,
					RefLon = beaconRef.RefLon,
					CurrentLat = current.Latitude,
					CurrentLon = current.Longitude,
					AidType = current.AidType,
					AidTypeID = current.AidTypeID,
					DateTimeUTC = beaconRef.CreatedAt,
					DistanceMeters = Math.Round(distance, 1),
					Time = DateTime.UtcNow,
					AisSentence = ais21,
				};

				await _alert.PushBroadcastAsync(alertPayload);
			}
			else if (distance <= threshold && beaconRef.IsDrifting)
			{
				beaconRef.IsDrifting = false;
				beaconRef.UpdatedAt = DateTime.UtcNow;
				await _context.SaveChangesAsync();
			}
		}
	}
}
