using System;

namespace ADLoginAPI.Models
{
	public class PAI_EmployeesActivitiesRowsFinal
	{
		public short ActivityYear { get; set; }
		public short Week { get; set; }
		public string? PICEmployee { get; set; }
		public DateTime RecordingDate { get; set; }
		public short RowID { get; set; }
		public string? TaskCode { get; set; }
		public double? ActivityTime { get; set; }
		public string? Description { get; set; }
		public int? LinkDetail { get; set; }
		public string? ProductCode { get; set; }
		public string? ReleaseNr { get; set; }
		public string? CompanyCode { get; set; }
		public DateTime TBCreated { get; set; }
		public DateTime TBModified { get; set; }
		public int? ActivityDoc { get; set; }
		public int TBCreatedID { get; set; }
		public int TBModifiedID { get; set; }
		public string? FunctionalAreas { get; set; }
		public string? TaskCodeDescription { get; set; }
		public int ActivityType { get; set; }
    }
}
