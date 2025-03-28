using myAISapi.Models;
using myAISapi.Services;

namespace myAISapi.Data
{
	public class DataHelper
	{
		public static object Ship(DecodedAISMessage message)
		{
			try
			{
				double SLength = message.DimensionToBow + message.DimensionToStern;
				double SWidth = message.DimensionToPort + message.DimensionToStar;
				string? name = message.VesselName;
				if (message.NameExtension != null) {
					name = name + message.NameExtension; 
				}
				name = name?.Replace("@","");
				string? callsign = message.CallSign?.Replace("@", "");

				DM_Tau tau = new DM_Tau
				{
					MMSI = message.MMSI,
					VesselName = name,
					IMONumber = message.IMONumber,
					CallSign = message.CallSign,
					ShipType = message.ShipType,
					AISVersion = message.AISVersion,
					TypeOfEPFD = message.TypeOfEPFD,
					DimensionToBow = message.DimensionToBow,
					DimensionToStern = message.DimensionToStern,
					DimensionToPort = message.DimensionToPort,
					DimensionToStar = message.DimensionToStar,
					ShipLength = SLength,
					ShipWidth = SWidth,
					Draught = message.Draught,
					Destination = message.Destination,
					AidType = message.AidType,
					VirtualAidFlag = message.VirtualAidFlag,
					OffPositionIndicator = message.OffPositionIndicator
				};
				return tau;
			}

			catch (Exception ex)
			{
				return $"Error parsing ship info: {ex.Message}";
			}


		}

		public static object Route(DecodedAISMessage message)
		{
			int ETAyear = message.ETAMonth > DateTime.Now.Month ? DateTime.Now.Year : DateTime.Now.Year + 1;
			try
			{
				DateTime? dtETA = null;
				if (message.ETAMonth != -1 && message.ETAMonth != 0 
					&& message.ETADay != -1 &&  message.ETADay != 0 
					&& message.ETAHour != -1 && message.ETAHour != 0
					&& message.ETAMinute != -1 && message.ETAMinute != 0)
				{
					dtETA = new DateTime(ETAyear, message.ETAMonth, message.ETADay, message.ETAHour, message.ETAMinute, DateTime.Now.Second);
				}
				DateTime? dtUTC = null;
				if (message.SecondUTC != -1)
				{
					DateTime baseTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
					if (message.SecondUTC > DateTime.Now.Second)
					{
						baseTime = baseTime.AddMinutes(-1); // Giảm phút xuống một đơn vị
					}

					dtUTC = baseTime.AddSeconds(message.SecondUTC ?? 0);
				}
				DM_HanhTrinh hanhtrinh = new DM_HanhTrinh
				{
					MMSI = message.MMSI,
					NavigationStatus = message.NavigationStatus,
					RateOfTurn = message.RateOfTurn,
					SpeedOverGround = message.SpeedOverGround,
					PositionAccuracy = message.PositionAccuracy,
					Longitude = message.Longitude,
					Latitude = message.Latitude,
					CourseOverGround = message.CourseOverGround,
					TrueHeading = message.TrueHeading,
					DateTimeUTC = dtUTC,
					ManeuverIndicator = message.ManeuverIndicator,
					RAIMFlags = message.RAIMFlags,
					PositionFixType = message.PositionFixType,
					StationType = message.StationType,
					ReportInterval = message.ReportInterval,
					DisplayFlag = message.DisplayFlag,
					ETADateTime = dtETA,
					DSCFlag = message.DSCFlag,
				};
				return hanhtrinh;
			}
			catch (Exception ex)
			{
				return $"Error parsing, {message.SecondUTC} route info: {ex.Message}, eyy {ETAyear}, eMM {message.ETAMonth}, edd {message.ETADay}, ehh {message.ETAHour}, emm {message.ETAMinute} , ess{DateTime.Now.Second} ";
			}
		}
	}
}
