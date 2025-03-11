namespace myAISapi.Models
{
	public class DecodedAISMessage
	{
		public int MessageType { get; set; }
		public int RepeatIndicator { get; set; }
		public int MMSI { get; set; }
		public string? VesselName { get; set; }
		public int IMONumber { get; set; }
		public string? CallSign { get; set; }
		public int ShipType { get; set; }
		public int NavigationStatus { get; set; }
		public double RateOfTurn { get; set; }
		public double SpeedOverGround { get; set; }
		public bool PositionAccuracy { get; set; }
		public double Longitude { get; set; }
		public double Latitude { get; set; }
		public double NELongitude { get; set; }
		public double NELatitude { get; set; }
		public double SWLongitude { get; set; }
		public double SWLatitude { get; set; }
		public double CourseOverGround { get; set; }
		public int TrueHeading { get; set; }
		public int Timestamp { get; set; }
		public int ManeuverIndicator { get; set; }
		public bool RAIMFlags { get; set; }
		public bool FixQuality { get; set; }
		public int AISVersion { get; set; }
		public int TypeOfEPFD { get; set; }
		public int SOTDMAstate { get; set; }
		public int RadioStatus { get; set; }
		public int PositionFixType { get; set; }
		public int RegionalReserved { get; set; }
		public int RegionalReserved2 { get; set; }
		public int StationType { get; set; }
		public int TxRxMode { get; set; }
		public int ReportInterval { get; set; }
		public int QuietTime { get; set; }
		public object? Spare2 { get; set; }
		public object? Spare3 { get; set; }
		public int PartNumber { get; set; }
		public string? VendorID { get; set; }
		public int UnitModelCode { get; set; }
		public string? SerialNumber { get; set; }
		public int MotherShipMMSI { get; set; }

		public bool DTE { get; set; }
		public string? Name { get; set; }
		public bool CSUnit { get; set; }
		public bool DisplayFlag { get; set; }
		public bool DSCFlag { get; set; }
		public bool BandFlag { get; set; }
		public bool Message22Flag { get; set; }
		public bool Assigned { get; set; }
		public bool AssignedModeFlag { get; set; }
		public int AidType { get; set; }
		public bool OffPositionIndicator { get; set; }
		public bool VirtualAidFlag { get; set; }
		public string? NameExtension { get; set; }
		public object? Spare { get; set; }
		public int YearUTC { get; set; }
		public int MonthUTC { get; set; }
		public int DayUTC { get; set; }
		public int HourUTC { get; set; }
		public int MinuteUTC { get; set; }
		public int? SecondUTC { get; set; }
		public int DimensionToBow { get; set; }
		public int DimensionToStern { get; set; }
		public int DimensionToPort { get; set; }
		public int DimensionToStar { get; set; }
		public int ETAMonth { get; set; }
		public int ETADay { get; set; }
		public int ETAHour { get; set; }
		public int ETAMinute { get; set; }
		public int Draught { get; set; }
		public string? Destination { get; set; }

	}
}
