using System;

namespace ADLoginAPI.DTO
{
    public class ActivityRowDTO
    {
        public int ActivityYear { get; set; }
        public int Week { get; set; }
        public required string PICEmployee { get; set; }
        public required DateTime RecordingDate { get; set; }
        public required int RowID { get; set; }
        public required string TaskCode { get; set; }
        public required string TaskCodeDescription { get; set; }
        public double? ActivityTime { get; set; }
        public required string Description { get; set; }
        public int? LinkDetail { get; set; }
        public required string ProductCode { get; set; }
        public required string ReleaseNr { get; set; }
        public required string CompanyCode { get; set; }
        //public int ActivityType { get; set; }
    }
}
