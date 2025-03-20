using System;

namespace ADLoginAPI.Models
{
    public class paragraph
    {
        public int id { get; set; }
        public int version_id { get; set; }
        public int type { get; set; }
        public string title { get; set; }
        public string title_en { get; set; }
        public string content { get; set; }
        public int progressive { get; set; }

        public virtual mago_version version { get; set; }
    }
}
