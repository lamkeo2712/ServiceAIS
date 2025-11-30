namespace myAISapi.Models
{
	public class BeaconRef
	{
		public int Id { get; set; }
		public int MMSI { get; set; }
		public string? BeaconName { get; set; }
		public double RefLat { get; set; }
		public double RefLon { get; set; }
		public double DriftThresholdMeters { get; set; } = 50;
		public bool IsDrifting { get; set; }
		public DateTime? LastAlertAt { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? UpdatedAt { get; set; }
	}
}
