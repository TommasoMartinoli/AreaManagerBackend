using System;
using System.Collections.Generic;

namespace ADLoginAPI.Models
{
    public class module_main
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? short_description { get; set; }
        public string? folders_application { get; set; }
        public string? folders_module { get; set; }
        public string? option_id { get; set; }
        public string? cal_type { get; set; }
        public byte available { get; set; }
        public byte available_cloud { get; set; }
        public byte available_web { get; set; }
        public int app_area { get; set; }
        public int progressive {  get; set; }

        public virtual area? area { get; set; }

        public string? mago_code { get; set; }
        public string? mago_web_code { get; set; }
        public string? mago_cloud_code { get; set; }

        public virtual List<module_edition> editions { get; set; } = new List<module_edition>();


        public virtual List<module_description> description_info { get; set; } = new List<module_description>();
        public virtual List<module_feature> features { get; set; } = new List<module_feature>();

        public string? mago_rel_version { get; set; }
        public DateTime? mago_valid_date { get; set; }
        public string? mago_solution { get; set; }
        public string? mago_sales_module { get; set; }
        public string? mago_allow_iso { get; set; }
        public string? mago_deny_iso { get; set; }
        public string? mago_sales_module_dependency { get; set; }
        public string? mago_product_id { get; set; }
        public string? mago_short_names { get; set; }

        public virtual List<module_x_package_mago> module_packages_mago { get; set; } = new();

        public string? cloud_rel_version { get; set; }
        public DateTime? cloud_valid_date { get; set; }
        public string? cloud_fragment_name { get; set; }
        public string? cloud_allow_iso { get; set; }
        public string? cloud_deny_iso { get; set; }
        public string? cloud_prerequisites { get; set; }
        public string? cloud_incompatibilities { get; set; }

        public virtual List<module_x_industry_cloud> module_industries_cloud { get; set; } = new();

        public string? web_rel_version { get; set; }
        public DateTime? web_valid_date { get; set; }
        public string? web_fragment_name { get; set; }
        public string? web_allow_iso { get; set; }
        public string? web_deny_iso { get; set; }
        public string? web_prerequisites { get; set; }
        public string? web_incompatibilities { get; set; }

        public virtual List<module_x_package_web> module_packages_web { get; set; } = new();
    }
}
