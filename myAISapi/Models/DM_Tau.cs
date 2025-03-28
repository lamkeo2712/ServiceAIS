using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace myAISapi.Models
{
	public class DM_Tau
	{
		[Key]
		public int MMSI { get; set; }
		public string? VesselName { get; set; }
		public int? IMONumber { get; set; }
		public string? CallSign { get; set; }
		public int? ShipType { get; set; }
		public int? AISVersion { get; set; }
		public int? TypeOfEPFD { get; set; }
		[NotMapped]
		public double? DimensionToBow { get; set; }
		[NotMapped]
		public double? DimensionToStern { get; set; }
		[NotMapped]
		public double? DimensionToPort { get; set; }
		[NotMapped]
		public double? DimensionToStar { get; set; }
		public double? ShipLength { get; set; }
		public double? ShipWidth { get; set; }
		public int? Draught { get; set; }
		public string? Destination { get; set; }
		public int? AidType { get; set; }
		public bool? VirtualAidFlag { get; set; }
		public bool? OffPositionIndicator { get; set; }
		public DateTime? CreatedAt { get; set; } 
		public DateTime? UpdatedAt { get; set; }
	}
}
