using System;

namespace ADLoginAPI.Models
{
    public class html_page
    {
        public int id { get; set; }
        public string? title { get; set; }
        public string? description { get; set; }
        public string? content { get; set; }
        public int? category_id { get; set; }
        public DateTime? creation_date { get; set; }

        public virtual html_category category { get; set; }
    }
}
