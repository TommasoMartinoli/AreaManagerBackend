using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADLoginAPI.Models
{
	public class PAI_EmployeesMaster
	{
		public required string PICEmployee { get; set; }
		public required string Name { get; set; }
		public required string  Surname { get; set; }
		public required string Sex { get; set; }
		public required string Phone { get; set; }
		public required string Fax { get; set; }
		public required string MobilePhone { get; set; }
		public required string PhoneExtension { get; set; }
		public required string EMail { get; set; }
		public required string SkypeID { get; set; }
		public int? Division { get; set; } = 39256064;
		public required string Notes { get; set; }
		public required string TrainingNotes { get; set; }
		public required string Disabled { get; set; } = "0";
		public required string CRMUserName { get; set; }
		public required string EmployeeTitle { get; set; }
		public required string PersonalDataAgreeInt { get; set; } = "1";
		public required string PersonalDataAgreeExt { get; set; } = "1";
		public required string ResellerFrontEnd { get; set; } = "0";
		public required string EndUserFrontEnd { get; set; } = "0";
		public required string CustomerCode { get; set; }
		public int? CompanyType { get; set; } = 36700161;
		public required DateTime TBCreated { get; set; } = DateTime.Now;
		public required DateTime TBModified { get; set; } = DateTime.Now;
		public short? HoursPerWeek { get; set; }
		public required string IdNumber { get; set; }
		public required string Salesperson { get; set; } = "NONE";
		public required string AreaManager { get; set; } = "NONE";
		public int? SalesArea { get; set; } = 33685510;
		public required int TBCreatedID { get; set; } = 0;
		public required int TBModifiedID { get; set; } = 0;
		public required string LimitedEducationArea { get; set; } = "0";
		public required string IsPublicCompany { get; set; }
		public int? WorkerID { get; set; }
		public required Guid TBGuid { get; set; } = Guid.NewGuid();
		public int? Job { get; set; } = 39256064;
	}
}
