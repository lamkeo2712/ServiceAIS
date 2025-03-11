namespace myAISapi.Models
{
	public class DM_Tau
	{
		public int MaHanhTrinh { get; set; }
		public int MMSI { get; set; }
		public string? VesselName { get; set; }
		public int? IMONumber { get; set; }
		public string? CallSign { get; set; }
		public int? ShipType { get; set; }
		public int? AISVersion { get; set; }
		public int? TypeOfEPFD { get; set; }
		public double? DimensionToBow { get; set; }
		public double? DimensionToStern { get; set; }
		public double? DimensionToPort { get; set; }
		public double? DimensionToStar { get; set; }
		public double? ShipLength { get; set; }
		public double? ShipWidth { get; set; }
		public int? Draught { get; set; }
		public string? Destination { get; set; }
		public bool? VirtualAidFlag { get; set; }
		public bool? OffPositionIndicator { get; set; }
	}
}
