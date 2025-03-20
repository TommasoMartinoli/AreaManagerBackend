using System;

namespace ADLoginAPI.Models
{
    public class fiscal_localization
    {
        public int? id { get; set; }
        public int version_id { get; set; }
        public string value { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }

        //public virtual mago_version version { get; set; }
    }
}