namespace myAISapi.Models
{
	public class BeaconCurrentDto
	{
		public int MMSI { get; set; }
		public double? Latitude { get; set; }
		public double? Longitude { get; set; }
		public string? VesselName { get; set; }
		public string? AidType { get; set; }
		public int AidTypeID { get; set; }
		public DateTime? DateTimeUTC { get; set; }
	}
}
