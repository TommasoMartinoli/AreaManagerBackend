using System;

namespace ADLoginAPI.Models
{
	public class PAI_EmployeesActivitiesPrivateNotes
	{
		public short ActivityYear { get; set; }
		public short Week { get; set; }
		public string? PICEmployee { get; set; }
		public required string IsTMNote { get; set; }
		public string? Notes { get; set; }
		public DateTime TBCreated { get; set; }
		public DateTime TBModified { get; set; }
		public string? TeamRole { get; set; }
		public string? PICWriter { get; set; }
	}


    public class PAI_EmployeesActivitiesPrivateNotes_Count
    {
        public int CountResult { get; set; }
    }

}
