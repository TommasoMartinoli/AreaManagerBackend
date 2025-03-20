namespace ADLoginAPI.DTO
{
    public class PrivateNoteDTO
    {
        public short ActivityYear { get; set; }
        public short Week { get; set; }
        public required string PICEmployee { get; set; }
        public required string IsTMNote { get; set; }
        public string? Notes { get; set; }
        public required string TeamRole { get; set; }
        public required string PICWriter { get; set; }
    }
}
