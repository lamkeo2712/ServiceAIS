using System.ComponentModel.DataAnnotations;

namespace myAISapi.Models
{
	public class DM_HanhTrinh
	{
		[Key]
		public int MaHanhTrinh { get; set; }
		public int MMSI { get; set; }
		public int? NavigationStatus { get; set; }
		public double? RateOfTurn { get; set; }
		public double? SpeedOverGround { get; set; }
		public bool? PositionAccuracy { get; set; }
		public double? Longitude { get; set; }
		public double? Latitude { get; set; }
		public double? CourseOverGround { get; set; }
		public int? TrueHeading { get; set; }
		public DateTime? DateTimeUTC { get; set; }
		public int? ManeuverIndicator { get; set; }
		public bool? RAIMFlags { get; set; }
		public int? PositionFixType { get; set; }
		public int? StationType { get; set; }
		public int? ReportInterval { get; set; }
		public bool? DisplayFlag { get; set; }
		public bool? DSCFlag { get; set; }
		public DateTime? ETADateTime { get; set; }
		public DateTime? CreatedAt { get; set; }
	}
}
