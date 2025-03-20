using System;

namespace ADLoginAPI.Models
{
	public class PAI_EmployeesActivities
	{
		public short ActivityYear { get; set; }
		public short Week { get; set; }
		public required string PICEmployee { get; set; }
		public string? PrevNotes { get; set; }
		public DateTime TBCreated { get; set; }
		public DateTime TBModified { get; set; }
		public int TBCreatedID { get; set; }
		public int TBModifiedID { get; set; }
		public Guid TBGuid { get; set; }
		public string? Notes { get; set; }
	}
}
