using static System.Runtime.InteropServices.JavaScript.JSType;
using System;
using System.Text;

namespace myAISapi.Decoder
{
	public class SharedFunction
	{
		//---------------------- Parse Data ----------------------

		public static string charTo6Bit(char @char)
		{
			// Bảng chuyển đổi ký tự AIS sang 6 bit
			const string aisChars = "0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW`abcdefghijklmnopqrstuvw";

			// Tìm vị trí của ký tự trong bảng AIS
			int index = aisChars.IndexOf(@char);

			if (index == -1)
			{
				throw new ArgumentException($"Ký tự không hợp lệ: {@char}");
			}

			// Chuyển đổi sang nhị phân 6 bit
			return Convert.ToString(index, 2).PadLeft(6, '0');
		}

		public static char binaryToAisChar(string binaryString)
		{
			const string aisChars = "0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW`abcdefghijklmnopqrstuvw";
			int decimalValue = Convert.ToInt32(binaryString, 2);
			return aisChars[decimalValue];
		}

		public static int binaryToSignedInt(string binaryString)
		{
			int bitLength = binaryString.Length;
			int value = Convert.ToInt32(binaryString, 2);
			if (value >= Math.Pow(2, bitLength - 1))
			{
				value -= (int)Math.Pow(2, bitLength);
			}
			return value;
		}

		public static object parseData(string binaryString, string type, int? scale = null, int? elementSize = null)
		{
			switch (type)
			{
				case "u":
					return Convert.ToInt32(binaryString, 2);
				case "i":
					int bitLength = binaryString.Length;
					int value = Convert.ToInt32(binaryString, 2);
					if (value >= Math.Pow(2, bitLength - 1))
					{
						value -= (int)Math.Pow(2, bitLength);
					}
					return value;
				case "U":
					long intValue = Convert.ToInt64(binaryString, 2);
					return scale.HasValue ? intValue / Math.Pow(10, scale.Value) : intValue;
				case "I":
					int intVal = binaryToSignedInt(binaryString);
					return scale.HasValue ? intVal / Math.Pow(10, scale.Value) : intVal;
				case "b":
					return binaryString == "1";
				case "e":
					return Convert.ToInt32(binaryString, 2);
				case "t":
					var chars = new StringBuilder();
					for (int i = 0; i < binaryString.Length; i += 6)
					{
						string chunk = binaryString.Substring(i, Math.Min(6, binaryString.Length - i));
						int val = Convert.ToInt32(chunk, 2);
						if (val < 32) val += 64; // AIS sử dụng khoảng giá trị này
						chars.Append((char)val);
					}
					return chars.ToString().Trim();
				case "d":
					return Enumerable.Range(0, binaryString.Length / 8)
						.Select(i => Convert.ToByte(binaryString.Substring(i * 8, 8), 2))
						.ToArray();
				case "a":
					if (!elementSize.HasValue)
						throw new ArgumentException("Element size is required for type 'a'");

					var array = new int[binaryString.Length / elementSize.Value];
					for (int i = 0; i < array.Length; i++)
					{
						string subStr = binaryString.Substring(i * elementSize.Value, elementSize.Value);
						array[i] = Convert.ToInt32(subStr, 2);
					}
					return array;
				case "x":
					return 0;
				default:
					return "Unknown type: " + type;
			}
		}

		//---------------------- Encoding function ----------------------

		public static string rateOfTurn(double ROT)
		{
			if (ROT > 0 && ROT <= 126)
				return $"turning right at {Math.Pow(ROT / 4.733, 2)} degrees per minute";
			else if (ROT < 0 && ROT >= -126)
				return $"turning left at {Math.Pow(ROT / 4.733, 2) * -1} degrees per minute or higher";
			else if (ROT == 0)
				return "not turning";
			else if (ROT == 127)
				return "turning right at more than 5deg/30s (No TI available)";
			else if (ROT == -127)
				return "turning left at more than 5deg/30s (No TI available)";
			else
				return "indicates no turn information available (default)";
		}
		public static string timeStamp(int TS)
		{
			return TS switch
			{
				60 => "time stamp is not available (default)",
				61 => "positioning system is in manual input mode",
				62 => "Electronic Position Fixing System operates in estimated (dead reckoning) mode",
				63 => "the positioning system is inoperative.",
				_ => TS.ToString()
			};
		}
		public static string trueHeading(int TH)
		{
			return TH >= 0 && TH <= 359 ? TH.ToString() : (TH == 511 ? "not available" : "");
		}

		public static string MMSI(int mmsi)
		{
			return mmsi.ToString().PadLeft(9, '0');
		}

		public static string altitude(int Alt)
		{
			return Alt == 4095 ? "not available" :
				   Alt == 4094 ? "4094m or higher" :
				   $"{Alt}m";
		}

		public static string SpeedOverGround(double SOG)
		{
			return SOG == 102.3 ? "speed not available" :
				   SOG == 102.2 ? "102.2 knots or higher" :
				   $"{SOG} knots";
		}

		public static string courseOverGround(double COG)
		{
			return COG == 3600 ? "course not available" : COG.ToString();
		}

		public static string Longitude(double lon)
		{
			try
			{
				lon = lon / 60;
				return lon == 181 ? "longitude is not available (default)" : lon.ToString();
			}
			catch (Exception ex)
			{
				return $"Error decoding Lon message: {ex.Message}";
			}
		}

		public static string Latitude(double lat)
		{
			try
			{
				lat = lat / 60;
				return lat == 91 ? "latitude is not available (default)" : lat.ToString();
			}
			catch (Exception ex)
			{
				return $"Error decoding Lat message: {ex.Message}";
			}
		}

		public static string SubVipPro(string input, int start, int length)
		{
			if (start >= input.Length)
				return "0";
			int editedLength = input.Length - start;
			return (start + length <= input.Length) ? input.Substring(start, length) : input.Substring(start, editedLength);
		}
		//---------------------- Constant Values ----------------------

		public static readonly string[] NavigationStatus = new[]
		{
			"Under Way Using Engine",
			"At Anchor",
			"Not Under Command",
			"Restricted manoeuvre",
			"Constrained by her draught",
			"Moored",
			"Aground",
			"Engaged in Fishing",
			"Under way sailing",
			"Reserved for future amendement of Navigational Status for HSC",
			"Reserved for future amendement of Navigational Status for WIG",
			"Power-driven vessel towing astern (regional use)",
			"Power-driven vessel towing pushing ahead or towing alongside (regional use)",
			"Reserved for future use",
			"AIS-SART is active",
			"Undefined (default)"
		};

		public static readonly string[] ManouverIndicator = new[]
		{
			"Not available (default)",
			"No special manoeuvre",
			"Special manoeuvre (such as regional passing arrangement)"
		};


		public static readonly string[] PositionFixType = new[]
{
	"undefined (default)",
	"GPS",
	"GLONASS",
	"Combined GPS/GLONASS",
	"Loran-C",
	"Chayka",
	"Intergrated navigation system",
	"Surveyed",
	"Galileo",
	"Reserved",
	"Reserved",
	"Reserved",
	"Reserved",
	"Reserved",
	"Reserved",
	"Internal GNSS"
};

		public static readonly string[] ShipType = new[]
	{
	"Not available (default)",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Reserved for future use",
	"Wing in ground (WIG), all ships of this type",
	"Wing in ground (WIG), Hazardous category A",
	"Wing in ground (WIG), Hazardous category B",
	"Wing in ground (WIG), Hazardous category C",
	"Wing in ground (WIG), Hazardous category D",
	"Wing in ground (WIG), Reserved for future use",
	"Wing in ground (WIG), Reserved for future use",
	"Wing in ground (WIG), Reserved for future use",
	"Wing in ground (WIG), Reserved for future use",
	"Wing in ground (WIG), Reserved for future use",
	"Fishing",
	"Towing",
	"Towing: length exceeds 200m or breadth exceeds 25m",
	"Dredging or underwater ops",
	"Driving ops",
	"Military ops",
	"Sailing",
	"Pleasure Craft",
	"Reserved",
	"Reserved",
	"High speed craft (HSC), all ships of this type",
	"High speed craft (HSC), Hazardous category A",
	"High speed craft (HSC), Hazardous category B",
	"High speed craft (HSC), Hazardous category C",
	"High speed craft (HSC), Hazardous category D",
	"High speed craft (HSC), Reserved for future use",
	"High speed craft (HSC), Reserved for future use",
	"High speed craft (HSC), Reserved for future use",
	"High speed craft (HSC), Reserved for future use",
	"High speed craft (HSC), Reserved for future use",
	"High speed craft (HSC), No additional information",
	"Pilot Vessel",
	"Search and Rescue vessel",
	"Tug",
	"Port Tender",
	"Anti Poluttion Equipment",
	"Law Enforcement",
	"Spare-Local Vessel",
	"Spare-Local Vessel",
	"Medical Transport",
	"Noncombatant ship according to RR Resolution No. 18",
	"Passenger, all ships of this type",
	"Passenger, Hazardous category A",
	"Passenger, Hazardous category B",
	"Passenger, Hazardous category C",
	"Passenger, Hazardous category D",
	"Passenger, Reserved for future use",
	"Passenger, Reserved for future use",
	"Passenger, Reserved for future use",
	"Passenger, Reserved for future use",
	"Passenger, No additional information",
	"Cargo, all ships of this type",
	"Cargo, Hazardous category A",
	"Cargo, Hazardous category B",
	"Cargo, Hazardous category C",
	"Cargo, Hazardous category D",
	"Cargo, Reserved for future use",
	"Cargo, Reserved for future use",
	"Cargo, Reserved for future use",
	"Cargo, Reserved for future use",
	"Cargo, No additional information",
	"Tanker, all ships of this type",
	"Tanker, Hazardous category A",
	"Tanker, Hazardous category B",
	"Tanker, Hazardous category C",
	"Tanker, Hazardous category D",
	"Tanker, Reserved for future use",
	"Tanker, Reserved for future use",
	"Tanker, Reserved for future use",
	"Tanker, Reserved for future use",
	"Tanker, No additional information",
	"Other Type, all ships of this type",
	"Other Type, Hazardous category A",
	"Other Type, Hazardous category B",
	"Other Type, Hazardous category C",
	"Other Type, Hazardous category D",
	"Other Type, Reserved for future use",
	"Other Type, Reserved for future use",
	"Other Type, Reserved for future use",
	"Other Type, Reserved for future use",
	"Other Type, no additional information"
};

		public static readonly string[] NavaidTypes = new[]
{
	"Default, Type of Aid to Navigation not specified",
	"Reference point",
	"RACON (radar transponder marking a navigation hazard)",
	"Fixed structure off shore, such as oil platforms, wind farms, rigs",
	"Spare, Reserved for future use",
	"Light, without sectors",
	"Light, with sectors",
	"Leading Light Front",
	"Leading Light Rear",
	"Beacon, Cardinal N",
	"Beacon, Cardinal E",
	"Beacon, Cardinal S",
	"Beacon, Cardinal W",
	"Beacon, Port hand",
	"Beacon, Starboard hand",
	"Beacon, Preferred Channel port hand",
	"Beacon, Preferred Channel starboard hand",
	"Beacon, Isolated danger",
	"Beacon, Safe water",
	"Beacon, Special mark",
	"Cardinal Mark N",
	"Cardinal Mark E",
	"Cardinal Mark S",
	"Cardinal Mark W",
	"Port hand Mark",
	"Starboard hand Mark",
	"Preferred Channel Port hand",
	"Preferred Channel Starboard hand",
	"Isolated danger",
	"Safe Water",
	"Special Mark",
	"Light Vessel / LANBY / Rigs"
};

		public static readonly string[] StationTypes = new[] {
			"All types of mobiles",
			"Reserved for future use",
			"All types of Class B mobile stations",
			"SAR airborne mobile station",
			"Aid to Navigation station",
			"Class B shipborne mobile station (IEC62287 only)",
			"Regional use and inland waterways",
			"Regional use and inland waterways",
			"Regional use and inland waterways",
			"Regional use and inland waterways",
			"Reserved for future use",
			"Reserved for future use",
			"Reserved for future use",
			"Reserved for future use",
			"Reserved for future use",
			"Reserved for future use"

		};

		public static readonly string[] TransmitModes = new[] {
			"TxA/TxB, RxA/RxB (default)",
			"TxA, RxA/RxB",
			"TxB, RxA/RxB",
			"Reserved for Future Use"
		};

		public static readonly string[] StationIntervals = new[] {
			"As given by the autonomous mode",
			"10 Minutes",
			"6 Minutes",
			"3 Minutes",
			"1 Minute",
			"30 Seconds",
			"15 Seconds",
			"10 Seconds",
			"5 Seconds",
			"Next Shorter Reporting Intervals",
			"Next Longer Reporting Intervals",
			"Reserved for Future Use",
		};


	}
}
