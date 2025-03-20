using System;

namespace ADLoginAPI.Models
{
    public class module_description
    {
        public int id { get; set; }
        public int? module_id { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
        public string? language { get; set; }
    }
}
