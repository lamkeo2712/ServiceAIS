using myAISapi.Models;
using System;
using static myAISapi.Decoder.SharedFunction;

namespace myAISapi.Decoder
{
	public class MessageTypes
	{
		public static object Type123(string payloadBit)
		{
			try
			{
				DecodedAISMessage dataDecode = new DecodedAISMessage
				{
					MessageType = (int)parseData(SubVipPro(payloadBit, 0, 6), "u"),
					RepeatIndicator = (int)parseData(SubVipPro(payloadBit, 6, 2), "u"),
					MMSI = (int)parseData(SubVipPro(payloadBit, 8, 30), "u"),
					NavigationStatus =(int)parseData(SubVipPro(payloadBit, 38, 4), "e"),
					RateOfTurn = (double)parseData(SubVipPro(payloadBit, 42, 8), "I", 3),
					SpeedOverGround = (double)parseData(SubVipPro(payloadBit, 50, 10), "U", 1),
					PositionAccuracy = (bool)parseData(SubVipPro(payloadBit, 60, 1), "b"),
					Longitude = (double)parseData(SubVipPro(payloadBit, 61, 28), "I", 4)/60,
					Latitude = (double)parseData(SubVipPro(payloadBit, 89, 27), "I", 4)/60,
					CourseOverGround = (double)parseData(SubVipPro(payloadBit, 116, 112), "U", 1),
					TrueHeading = (int)parseData(SubVipPro(payloadBit, 128, 9), "u"),
					SecondUTC = (int)parseData(SubVipPro(payloadBit, 137, 6), "u") is var second && (int)second >= 0 && (int)second <= 59 ? second : -1,
					ManeuverIndicator = (int)parseData(SubVipPro(payloadBit, 143, 2), "e"),
					Spare = null,
					RAIMFlags = (bool)parseData(SubVipPro(payloadBit, 148, 1), "b") ,
					RadioStatus = (int)parseData(SubVipPro(payloadBit, 149, 19), "u")
				};
				return dataDecode;
			}
			catch (Exception ex)
			{
				return $"Error decoding Type 1/2/3 message: {ex.Message}";
			}
		}

		public static object Type4(string payloadBit)
		{
			try
			{
				DecodedAISMessage dataDecode = new DecodedAISMessage
				{
					MessageType = (int)parseData(SubVipPro(payloadBit,0, 6), "u"),
					RepeatIndicator = (int)parseData(SubVipPro(payloadBit,6, 2), "u"),
					MMSI = (int)parseData(SubVipPro(payloadBit,8, 30), "u"),
					YearUTC = (int)parseData(SubVipPro(payloadBit,38, 14), "u") is var year && year >= 1 && year <= 9999 ? year : -1,
					MonthUTC = (int)parseData(SubVipPro(payloadBit,52, 4), "u") is var month && (int)month >= 1 && (int)month <= 12 ? month : -1,
					DayUTC = (int)parseData(SubVipPro(payloadBit,56, 5), "u") is var day && (int)day >= 1 && (int)day <= 31 ? day : -1,
					HourUTC = (int)parseData(SubVipPro(payloadBit,61, 5), "u") is var hour && (int)hour >= 0 && (int)hour <= 23 ? hour : -1,
					MinuteUTC = (int)parseData(SubVipPro(payloadBit,66, 6), "u") is var minute && (int)minute >= 0 && (int)minute <= 59 ? minute : -1,
					SecondUTC = (int)parseData(SubVipPro(payloadBit,72, 6), "u") is var second && (int)second >= 0 && (int)second <= 59 ? second : -1,
					FixQuality = (bool)parseData(SubVipPro(payloadBit,78, 1), "b"),
					Longitude = (double)parseData(SubVipPro(payloadBit,79, 28), "I", 4)/60,
					Latitude = (double)parseData(SubVipPro(payloadBit,107, 27), "I", 4)/60,
					TypeOfEPFD = (int)parseData(SubVipPro(payloadBit,134, 4), "e"),
					Spare = null, // Thay thế cho 'x'
					RAIMFlags = (bool)parseData(SubVipPro(payloadBit,148, 1), "b"),
					SOTDMAstate = (int)parseData(SubVipPro(payloadBit,149, 19), "u")
				};
				return dataDecode;
			}
			catch (Exception ex)
			{
				return $"Error decoding Type 4 message: {ex.Message}";
			}
		}

		public static object Type5(string payloadBit)
		{
			try
			{
				DecodedAISMessage dataDecode = new DecodedAISMessage
				{
					MessageType = (int)parseData(SubVipPro(payloadBit,0, 6), "u"),
					RepeatIndicator = (int)parseData(SubVipPro(payloadBit,6, 2), "u"),
					MMSI = (int)parseData(SubVipPro(payloadBit,8, 30), "u"),
					AISVersion = (int)parseData(SubVipPro(payloadBit,38, 2), "u"),
					IMONumber = (int)parseData(SubVipPro(payloadBit,40, 30), "u"),
					CallSign = (string)parseData(SubVipPro(payloadBit,70, 42), "t"),
					VesselName = (string)parseData(SubVipPro(payloadBit,112, 120), "t"),
					ShipType = (int)parseData(SubVipPro(payloadBit,232, 8), "e"),
					DimensionToBow = (int)parseData(SubVipPro(payloadBit,240, 9), "u"),
					DimensionToStern = (int)parseData(SubVipPro(payloadBit, 249, 9), "u"),
					DimensionToPort = (int)parseData(SubVipPro(payloadBit, 258, 6), "u"),
					DimensionToStar = (int)parseData(SubVipPro(payloadBit, 264, 6), "u"),
					PositionFixType = (int)parseData(SubVipPro(payloadBit, 270, 4), "e"),
					ETAMonth = (int)parseData(SubVipPro(payloadBit,274, 4), "u") is var month && month >= 1 && month <= 12 ? month : -1,
					ETADay = (int)parseData(SubVipPro(payloadBit,278, 5), "u") is var day && day >= 1 && day <= 31 ? day : -1,
					ETAHour = (int)parseData(SubVipPro(payloadBit,283, 5), "u") is var hour && hour >= 0 && hour <= 23 ? hour : -1,
					ETAMinute = (int)parseData(SubVipPro(payloadBit, 288, 6), "u") is var minute && minute >= 0 && minute <= 59 ? minute : -1,
					Draught = (int)parseData(SubVipPro(payloadBit,294, 8), "U", 1),
					Destination = (string)parseData(SubVipPro(payloadBit,302, 120), "t"),
					DTE = (bool)parseData(SubVipPro(payloadBit,422, 1), "b"),
					Spare = null // Thay thế cho 'x'
				};
				return dataDecode;
			}
			catch (Exception ex)
			{
				return $"Error decoding Type 5 message: {ex.Message} , payloadBit length: {payloadBit.Length}";
			}
		}


		public static object Type18(string payloadBit)
		{
			try
			{
				DecodedAISMessage dataDecode = new DecodedAISMessage
				{
					MessageType = (int)parseData(SubVipPro(payloadBit,0, 6), "u"),
					RepeatIndicator = (int)parseData(SubVipPro(payloadBit,6, 2), "u"),
					MMSI = (int)parseData(SubVipPro(payloadBit,8, 30), "u"),
					RegionalReserved = (int)parseData(SubVipPro(payloadBit,38, 8), "x"),
					SpeedOverGround = (int)parseData(SubVipPro(payloadBit,46, 10), "U", 1),
					PositionAccuracy = (bool)parseData(SubVipPro(payloadBit,56, 1), "b"),
					Longitude = (double)parseData(SubVipPro(payloadBit,57, 28), "I", 4)/60,
					Latitude = (double)parseData(SubVipPro(payloadBit,85, 27), "I", 4)/60,
					CourseOverGround = (double)parseData(SubVipPro(payloadBit,112, 12), "U", 1),
					TrueHeading = (int)parseData(SubVipPro(payloadBit,124, 9), "u"),
					SecondUTC = (int)parseData(SubVipPro(payloadBit,133, 6), "u") is var second && (int)second >= 0 && (int)second <= 59 ? second : -1,
					RegionalReserved2 = (int)parseData(SubVipPro(payloadBit,139, 2), "u"),
					CSUnit = (bool)parseData(SubVipPro(payloadBit,141, 1), "b") ,
					DisplayFlag = (bool)parseData(SubVipPro(payloadBit,142, 1), "b") ,
					DSCFlag = (bool)parseData(SubVipPro(payloadBit,143, 1), "b") ,
					BandFlag = (bool)parseData(SubVipPro(payloadBit,144, 1), "b") ,
					Message22Flag = (bool)parseData(SubVipPro(payloadBit,145, 1), "b") ,
					Assigned = (bool)parseData(SubVipPro(payloadBit,146, 1), "b") ,
					RAIMFlags = (bool)parseData(SubVipPro(payloadBit,147, 1), "b") ,
					RadioStatus = (int)parseData(SubVipPro(payloadBit,148, 20), "u")
				};
				return dataDecode;
			}
			catch (Exception ex)
			{
				return $"Error decoding Type 18 message: {ex.Message}";
			}
		}

		public static object Type19(string payloadBit)
		{
			try
			{
				DecodedAISMessage dataDecode = new DecodedAISMessage
				{
					MessageType = (int)parseData(SubVipPro(payloadBit,0, 6), "u"),
					RepeatIndicator = (int)parseData(SubVipPro(payloadBit,6, 2), "u"),
					MMSI = (int)parseData(SubVipPro(payloadBit,8, 30), "u"),
					RegionalReserved = (int)parseData(SubVipPro(payloadBit,38, 8), "u"),
					SpeedOverGround = (double)parseData(SubVipPro(payloadBit,46, 10), "U", 1),
					PositionAccuracy = (bool)parseData(SubVipPro(payloadBit,56, 1), "b"),
					Longitude = (double)parseData(SubVipPro(payloadBit,57, 28), "I", 4)/60,
					Latitude = (double)parseData(SubVipPro(payloadBit,85, 27), "I", 4)/60,
					CourseOverGround = (double)parseData(SubVipPro(payloadBit,112, 12), "U", 1),
					TrueHeading = (int)parseData(SubVipPro(payloadBit,124, 9), "u"),
					SecondUTC = (int)parseData(SubVipPro(payloadBit,133, 6), "u") is var second && (int)second >= 0 && (int)second <= 59 ? second : -1,
					RegionalReserved2 = (int)parseData(SubVipPro(payloadBit,139, 4), "u"),
					Name = (string)parseData(SubVipPro(payloadBit,143, 120), "t"),
					ShipType = (int)parseData(SubVipPro(payloadBit,263, 8), "e"),
					DimensionToBow = (int)parseData(SubVipPro(payloadBit,271, 9), "u"),
					DimensionToStern = (int)parseData(SubVipPro(payloadBit,280, 9), "u"),
					DimensionToPort = (int)parseData(SubVipPro(payloadBit,289, 6), "u"),
					DimensionToStar = (int)parseData(SubVipPro(payloadBit,295, 6), "u"),
					PositionFixType = (int)parseData(SubVipPro(payloadBit,301, 4), "e"),
					RAIMFlags = (bool)parseData(SubVipPro(payloadBit,305, 1), "b"),
					DTE = (bool)parseData(SubVipPro(payloadBit,306, 1), "b"),
					AssignedModeFlag = (bool)parseData(SubVipPro(payloadBit,307, 1), "b"),
					Spare = null // Thay thế cho 'x'
				};
				return dataDecode;
			}
			catch (Exception ex)
			{
				return $"Error decoding Type 19 message: {ex.Message}";
			}
		}

		public static object Type21(string payloadBit)
		{
			try
			{
				DecodedAISMessage dataDecode = new DecodedAISMessage
				{
					MessageType = (int)parseData(SubVipPro(payloadBit,0, 6), "u"),
					RepeatIndicator = (int)parseData(SubVipPro(payloadBit,6, 2), "u"),
					MMSI = (int)parseData(SubVipPro(payloadBit,8, 30), "u"),
					AidType = (int)parseData(SubVipPro(payloadBit, 38, 5), "e"),
					Name = (string)parseData(SubVipPro(payloadBit,43, 120), "t"),
					PositionAccuracy = (bool)parseData(SubVipPro(payloadBit,163, 1), "b"),

					Longitude = (double)parseData(SubVipPro(payloadBit, 164, 28), "I", 4)/60,
					Latitude = (double)parseData(SubVipPro(payloadBit, 192, 27), "I", 4)/60,

					DimensionToBow = (int)parseData(SubVipPro(payloadBit,219, 9), "u"),
					DimensionToStern = (int)parseData(SubVipPro(payloadBit,228, 9), "u"),
					DimensionToPort = (int)parseData(SubVipPro(payloadBit, 237, 6), "u"),
					DimensionToStar = (int)parseData(SubVipPro(payloadBit, 243, 6), "u"),

					TypeOfEPFD = (int)parseData(SubVipPro(payloadBit,249, 4), "e"),

					SecondUTC = (int)parseData(SubVipPro(payloadBit, 253, 6), "u") is var second && (int)second >= 0 && (int)second <= 59 ? second : -1,

					OffPositionIndicator =  (bool)parseData(SubVipPro(payloadBit,259, 1), "b"),

					RegionalReserved = (int)parseData(SubVipPro(payloadBit,260, 8), "u"),

					RAIMFlags = (bool)parseData(SubVipPro(payloadBit, 268, 1), "b"),

					VirtualAidFlag = (bool)parseData(SubVipPro(payloadBit,269, 1), "b"),

					AssignedModeFlag = (bool)parseData(SubVipPro(payloadBit,270, 1), "b") ,

					Spare = null, // Thay thế cho 'x'

					NameExtension = (string)parseData(SubVipPro(payloadBit,272, 89), "t")
				};
				return dataDecode;
			}
			catch (Exception ex)
			{
				return $"Error decoding Type 21 message: {ex.Message} , payloadBit length: {payloadBit.Length}";
			}
		}


		public static object Type23(string payloadBit)
		{
			try
			{
				DecodedAISMessage dataDecode = new DecodedAISMessage
				{
					MessageType = (int)parseData(SubVipPro(payloadBit,0, 6), "u"),
					RepeatIndicator = (int)parseData(SubVipPro(payloadBit,6, 2), "u"),
					MMSI = (int)parseData(SubVipPro(payloadBit, 8, 30), "u"),
					Spare = null,
					NELongitude = (double)parseData(SubVipPro(payloadBit, 40, 18), "I", 4)/60,
					NELatitude = (double)parseData(SubVipPro(payloadBit, 58, 17), "I", 4)/60,
					SWLongitude = (double)parseData(SubVipPro(payloadBit, 75, 18), "I", 4) / 60,
					SWLatitude = (double)parseData(SubVipPro(payloadBit, 93, 17), "I", 4) / 60,
					StationType = (int)parseData(SubVipPro(payloadBit,110, 4), "e"),
					ShipType = (int)parseData(SubVipPro(payloadBit,114, 8), "e"),
					Spare2 = null,
					TxRxMode = (int)parseData(SubVipPro(payloadBit, 144, 2), "u"),
					ReportInterval = (int)parseData(SubVipPro(payloadBit, 146, 4), "u"),
					QuietTime = (int)parseData(SubVipPro(payloadBit,150, 4), "u") ,
					Spare3 = null
				};
				return dataDecode;
			}
			catch (Exception ex)
			{
				return $"Error decoding Type 23 message: {ex.Message}";
			}
		}


		public static object Type24(string payloadBit)
		{
			try
			{
				int PartNumber = (int)parseData(SubVipPro(payloadBit,38, 2), "u");

				DecodedAISMessage dataDecodeA = new DecodedAISMessage
				{
					MessageType = (int)parseData(SubVipPro(payloadBit,0, 6), "u"),
					RepeatIndicator = (int)parseData(SubVipPro(payloadBit,6, 2), "u"),
					MMSI = (int)parseData(SubVipPro(payloadBit,8, 30), "u"),
					PartNumber = (int)parseData(SubVipPro(payloadBit,38, 2), "u"),
					VesselName = (string)parseData(SubVipPro(payloadBit,40, 120), "t"),
					Spare = "Not using", // Thay thế cho 'x',
				};

				DecodedAISMessage dataDecodeB = new DecodedAISMessage
				{
					MessageType = (int)parseData(SubVipPro(payloadBit,0, 6), "u"),
					RepeatIndicator = (int)parseData(SubVipPro(payloadBit,6, 2), "u"),
					MMSI = (int)parseData(SubVipPro(payloadBit,8, 30), "u"),
					PartNumber = (int)parseData(SubVipPro(payloadBit,38, 2), "u"),
					ShipType = (int)parseData(SubVipPro(payloadBit,40, 8), "e"),
					VendorID = (string)parseData(SubVipPro(payloadBit,48, 18), "t"),
					UnitModelCode = (int)parseData(SubVipPro(payloadBit,66, 4), "u"),
					SerialNumber = (string)parseData(SubVipPro(payloadBit,70, 20), "t"),
					CallSign = (string)parseData(SubVipPro(payloadBit,90, 42), "t"),
					DimensionToBow = (int)parseData(SubVipPro(payloadBit,132, 9), "u"),
					DimensionToStern = (int)parseData(SubVipPro(payloadBit, 141, 9), "u"),
					DimensionToPort = (int)parseData(SubVipPro(payloadBit, 150, 6), "u"),
					DimensionToStar = (int)parseData(SubVipPro(payloadBit, 156, 6), "u"),
					MotherShipMMSI = (int)parseData(SubVipPro(payloadBit,132, 30), "u"),
					Spare = null, // Thay thế cho 'x',
				};

				if (PartNumber == 0)
				{
					return dataDecodeA;
				}
				else if (PartNumber == 1)
				{
					return dataDecodeB;
				}
				else
				{
					return $"Error decoding Type 24 message: Invalid Part Number {PartNumber}";
				}
			}
			catch (Exception ex)
			{
				return $"Error decoding Type 24 message: {ex.Message}";
			}
		}


	}
}
