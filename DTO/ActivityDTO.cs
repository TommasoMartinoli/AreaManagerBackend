namespace ADLoginAPI.DTO
{
    public class ActivityDTO
    {
        public int ActivityYear { get; set; }
        public int Week { get; set; }
        public required string PICEmployee { get; set; }
        public string? PrevNotes { get; set; }
        public string? Notes { get; set; }
    }
}
