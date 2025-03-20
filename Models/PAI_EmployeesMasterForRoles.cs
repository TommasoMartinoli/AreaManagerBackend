using System;

namespace ADLoginAPI.Models
{
    public class PAI_EmployeesMasterForRoles
    {
        public required string PICEmployee { get; set; }
        public required string Surname { get; set; }
        public required short HoursPerWeek { get; set; }
        public required string Name { get; set; }
    }
}
