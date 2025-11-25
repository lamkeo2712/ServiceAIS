using Cassandra;
using myAISapi.Models;

public class CassandraHanhTrinhRepository : ICassandraHanhTrinhRepository
{
	private readonly Cassandra.ISession _session;
	private readonly PreparedStatement _selectByMmsiDayStmt;
	private readonly PreparedStatement _insertStmt;

	public CassandraHanhTrinhRepository(Cassandra.ISession session)
	{
		_session = session;

		_selectByMmsiDayStmt = _session.Prepare(@"
			SELECT MaHanhTrinh,
				   MMSI,
				   MessageType,
				   NavigationStatus,
				   RateOfTurn,
				   SpeedOverGround,
				   PositionAccuracy,
				   Longitude,
				   Latitude,
				   CourseOverGround,
				   TrueHeading,
				   TimeStamp,
				   ManeuverIndicator,
				   RAIMFlags,
				   PositionFixType,
				   StationType,
				   ReportInterval,
				   ETADateTime,
				   DisplayFlag,
				   DSCFlag,
				   CreatedAt,
				   DateTimeUTC,
				   day
			FROM QL_HanhTrinh
			WHERE MMSI = ? AND day = ?;
		");

		_insertStmt = _session.Prepare(@"
            INSERT INTO QL_HanhTrinh (
                MaHanhTrinh,
                MMSI,
                MessageType,
                NavigationStatus,
                RateOfTurn,
                SpeedOverGround,
                PositionAccuracy,
                Longitude,
                Latitude,
                CourseOverGround,
                TrueHeading,
                TimeStamp,
                ManeuverIndicator,
                RAIMFlags,
                PositionFixType,
                StationType,
                ReportInterval,
                ETADateTime,
                DisplayFlag,
                DSCFlag,
                CreatedAt,
                DateTimeUTC,
                day
            ) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);
        ");
	}

	public async Task InsertBatchAsync(IEnumerable<DM_HanhTrinh> routes, CancellationToken ct = default)
	{
		// Lưu ý: Cassandra không khuyến khích LOGGED BATCH to đùng;
		// ở đây mình insert tuần tự, sau này tối ưu tiếp (group theo (MMSI, day) để dùng UNLOGGED BATCH).
		var tasks = new List<Task>();

		foreach (var r in routes)
		{
			if (!r.DateTimeUTC.HasValue)
				continue; // không có DateTimeUTC thì không partition được

			var dtUtc = DateTime.SpecifyKind(r.DateTimeUTC.Value, DateTimeKind.Utc);
			var day = new LocalDate(dtUtc.Year, dtUtc.Month, dtUtc.Day);

			// tạm auto MaHanhTrinh nếu = 0
			var maHanhTrinh = r.MaHanhTrinh != 0
				? r.MaHanhTrinh
				: (int)(dtUtc.Ticks % int.MaxValue);

			// Nếu bạn có MessageType trong DecodedAISMessage thì bổ sung vào DM_HanhTrinh,
			// còn hiện tại tạm để 0.
			int messageType = 0;

			var bound = _insertStmt.Bind(
				maHanhTrinh,
				r.MMSI,
				messageType,
				r.NavigationStatus ?? 0,
				r.RateOfTurn.HasValue ? (float?)r.RateOfTurn.Value : null,
				r.SpeedOverGround.HasValue ? (float?)r.SpeedOverGround.Value : null,
				r.PositionAccuracy,
				r.Longitude.HasValue ? (float?)r.Longitude.Value : null,
				r.Latitude.HasValue ? (float?)r.Latitude.Value : null,
				r.CourseOverGround.HasValue ? (float?)r.CourseOverGround.Value : null,
				r.TrueHeading,
				/* TimeStamp */ null, // nếu bạn có trường này trong DM_HanhTrinh thì map vào
				r.ManeuverIndicator,
				r.RAIMFlags,
				r.PositionFixType,
				r.StationType,
				r.ReportInterval,
				r.ETADateTime,
				r.DisplayFlag,
				r.DSCFlag,
				r.CreatedAt ?? DateTime.UtcNow,
				dtUtc,
				day
			);

			tasks.Add(_session.ExecuteAsync(bound));
		}

		await Task.WhenAll(tasks);
	}

	public async Task<IReadOnlyList<DM_HanhTrinh>> GetHanhTrinhAsync(
	int mmsi,
	int hours,
	CancellationToken ct = default)
	{
		var nowUtc = DateTime.Now;
		var startUtc = nowUtc.AddHours(-hours);

		var result = new List<DM_HanhTrinh>();

		// Lặp qua từng ngày trong khoảng [startUtc, nowUtc]
		var currentDate = startUtc.Date;
		var lastDate = nowUtc.Date;

		while (currentDate <= lastDate)
		{
			if (ct.IsCancellationRequested)
				break;

			// LocalDate cho partition key "day"
			var localDate = new LocalDate(currentDate.Year, currentDate.Month, currentDate.Day);

			// Query TẤT CẢ record của ngày đó cho MMSI (không filter DateTimeUTC ở CQL nữa)
			var bound = _selectByMmsiDayStmt.Bind(mmsi, localDate);
			var rs = await _session.ExecuteAsync(bound).ConfigureAwait(false);

			foreach (var row in rs)
			{
				var ht = MappingHanhTrinh(row);

				// Đảm bảo DateTimeUTC được coi là UTC
				if (ht.DateTimeUTC.HasValue)
				{
					var dtUtc = DateTime.SpecifyKind(ht.DateTimeUTC.Value, DateTimeKind.Utc);

					// Lọc theo khoảng giờ [startUtc, nowUtc]
					if (dtUtc < startUtc || dtUtc > nowUtc)
						continue;

					// Lọc rác: lat/lon null hoặc = 0
					if (!ht.Latitude.HasValue || ht.Latitude.Value == 0 ||
						!ht.Longitude.HasValue || ht.Longitude.Value == 0)
						continue;

					ht.DateTimeUTC = dtUtc; // gán lại cho chắc

					result.Add(ht);
				}
			}

			currentDate = currentDate.AddDays(1);
		}

		// Sắp xếp tăng dần giống proc SQL cũ
		return result
			.OrderBy(r => r.DateTimeUTC ?? DateTime.MinValue)
			.ToList();
	}

	private DM_HanhTrinh MappingHanhTrinh(Row row)
	{
		return new DM_HanhTrinh
		{
			MaHanhTrinh = row.GetValue<int>("mahanhtrinh"),
			MMSI = row.GetValue<int>("mmsi"),
			NavigationStatus = row.GetValue<int>("navigationstatus"),
			RateOfTurn = row.GetValue<float?>("rateofturn"),
			SpeedOverGround = row.GetValue<float?>("speedoverground"),
			PositionAccuracy = row.GetValue<bool?>("positionaccuracy"),
			Longitude = row.GetValue<float?>("longitude"),
			Latitude = row.GetValue<float?>("latitude"),
			CourseOverGround = row.GetValue<float?>("courseoverground"),
			TrueHeading = row.GetValue<int?>("trueheading"),
			ManeuverIndicator = row.GetValue<int?>("maneuverindicator"),
			RAIMFlags = row.GetValue<bool?>("raimflags"),
			PositionFixType = row.GetValue<int?>("positionfixtype"),
			StationType = row.GetValue<int?>("stationtype"),
			ReportInterval = row.GetValue<int?>("reportinterval"),
			ETADateTime = row.GetValue<DateTime?>("etadatetime"),
			DisplayFlag = row.GetValue<bool?>("displayflag"),
			DSCFlag = row.GetValue<bool?>("dscflag"),
			DateTimeUTC = row.GetValue<DateTime>("datetimeutc")
		};
	}
}
