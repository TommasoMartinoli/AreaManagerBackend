using System;

namespace ADLoginAPI.Models
{
    public class module_x_package_mago
    {
        public int module_id { get; set; }
        public int edition_id { get; set; }
        public int package_id { get; set; }
        public int status { get; set; }
        public int progressive { get; set; }

        public virtual module_main Module { get; set; }
        public virtual mago_edition Edition { get; set; }
        public virtual package Package { get; set; }
    }
}
