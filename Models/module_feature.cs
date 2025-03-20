using System;

namespace ADLoginAPI.Models
{
    public class module_feature
    {
        public int id { get; set; }
        public int? module_id { get; set; }
        public string? description { get; set; }
        public string? description_en { get; set; }
        public string? countries { get; set; }
        public string? deny_countries { get; set; }
        public int progressive { get; set; }
    }
}
