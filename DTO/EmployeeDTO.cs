using System;

namespace ADLoginAPI.DTO
{
    public class EmployeeDTO
    {
        public required string PICEmployee { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public short? HoursPerWeek { get; set; }
    }
}
