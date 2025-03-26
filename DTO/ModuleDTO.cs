using System;
using System.Collections.Generic;
using ADLoginAPI.Models;

namespace ADLoginAPI.DTO
{

    public class PageDTO
    {
        public int id { get; set; }
        public string? title { get; set; }
        public int? category_id { get; set; }
        public string? description { get; set; }
        public string? content { get; set; }
        public DateTime? creation_date { get; set; }
    }


    public class ModuleDTO
    {
        public int id { get; set; }
        public string? name { get; set; }
        public string? short_description { get; set; }
        public int app_area { get; set; }
        public string? option_id { get; set; }
        public int progressive { get; set; }
        public byte available { get; set; }
        public byte available_cloud { get; set; }
        public byte available_web { get; set; }
    }

    public class ModuleFeatureDTO
    {
        public int id { get; set; }
        public int? module_id { get; set; }
        public string description { get; set; } = string.Empty;
        public string description_en { get; set; } = string.Empty;
        public string countries { get; set; } = string.Empty;
        public string deny_countries { get; set; } = string.Empty;
        public int progressive { get; set; }
    }

    public class ModuleData
    {
        public required MainData main { get; set; }
        public required MagoData mago { get; set; }
        public required MagoCloudData magoCloud { get; set; }
        public required MagoWebData magoWeb { get; set; }
    }

    public class MainData
    {
        public int? id { get; set; }
        public string? name { get; set; }
        public string? short_description { get; set; }
        public string? folders_application { get; set; }
        public string? folders_module { get; set; }
        public string? option_id { get; set; }
        public string? cal_type { get; set; }
        public bool available { get; set; }
        public bool available_cloud { get; set; }
        public bool available_web { get; set; }
        public int app_area { get; set; }
        public string? mago_code { get; set; }
        public string? mago_cloud_code { get; set; }
        public string? mago_web_code { get; set; }

        public List<module_description> description_info { get; set; } = new List<module_description>();
        public List<ModuleFeatureDTO> features { get; set; } = new List<ModuleFeatureDTO>();
    }

    public class ModuleEditionDTO
    {
        public int edition_id { get; set; }
        public int module_id { get; set; }
        public int status { get; set; }
    }

    public class MagoData
    {
        public string? mago_rel_version { get; set; }
        public DateTime? mago_valid_date { get; set; }
        public string? mago_solution { get; set; }
        public string? mago_sales_module { get; set; }
        public string? mago_allow_iso { get; set; }
        public string? mago_deny_iso { get; set; }
        public string? mago_sales_module_dependency { get; set; }
        public string? mago_product_id { get; set; }
        public string? mago_short_names { get; set; }

        public List<ModulePackageMagoDTO>? selected_packages { get; set; } = new List<ModulePackageMagoDTO>();
        public List<ModuleEditionDTO>? mago_editions { get; set; } = new List<ModuleEditionDTO>();
    }

    public class MagoCloudData
    {
        public string? cloud_rel_version { get; set; }
        public DateTime? cloud_valid_date { get; set; }
        public string? cloud_fragment_name { get; set; }
        public string? cloud_allow_iso { get; set; }
        public string? cloud_deny_iso { get; set; }
        public string? cloud_prerequisites { get; set; }
        public string? cloud_incompatibilities { get; set; }

        public List<ModuleIndustryCloudDTO>? selected_industries { get; set; } = new List<ModuleIndustryCloudDTO>();
        public List<ModuleEditionDTO>? cloud_editions { get; set; } = new List<ModuleEditionDTO>();
    }

    public class MagoWebData
    {
        public string? web_rel_version { get; set; }
        public DateTime? web_valid_date { get; set; }
        public string? web_fragment_name { get; set; }
        public string? web_allow_iso { get; set; }
        public string? web_deny_iso { get; set; }
        public string? web_prerequisites { get; set; }
        public string? web_incompatibilities { get; set; }

        public List<ModulePackageWebDTO>? selected_packages { get; set; } = new List<ModulePackageWebDTO>();
        public List<ModuleEditionDTO>? web_editions { get; set; } = new List<ModuleEditionDTO>();
    }

    public class ModulePackageMagoDTO
    {
        public int edition_id { get; set; }
        public int module_id { get; set; }
        public int package_id { get; set; }
        public int status { get; set; } 
        public int progressive { get; set; }
    }

    public class ModulePackageWebDTO
    {
        public int edition_id { get; set; }
        public int module_id { get; set; }
        public int package_id { get; set; }
        public int status { get; set; }
        public int progressive { get; set; }
    }

    public class ModuleIndustryCloudDTO
    {
        public int edition_id { get; set; }
        public int module_id { get; set; }
        public int industry_id { get; set; }
        public int status { get; set; }
        public int progressive { get; set; }
    }

    public class AreaModulesDTO
    {
        public int id { get; set; } 
        public List<ModuleDTO> modules { get; set; } = new();
    }

    public class ModuleIndustryUpdateDTO
    {
        public int industry_id { get; set; }
        public int module_id { get; set; }
        public int edition_id { get; set; }
        public int status { get; set; }
        public int progressive { get; set; }
    }

    public class ModulePackageMagoUpdateDTO
    {
        public int package_id { get; set; }
        public int module_id { get; set; }
        public int edition_id { get; set; }
        public int status { get; set; }
        public int progressive { get; set; }
    }

    public class ModulePackageWebUpdateDTO
    {
        public int package_id { get; set; }
        public int module_id { get; set; }
        public int edition_id { get; set; }
        public int status { get; set; }
        public int progressive { get; set; }
    }

    public class ParagraphDTO
    {
        public int? id { get; set; }  
        public int type { get; set; } 
        public string title { get; set; }
        public string title_en { get; set; }
        public string content { get; set; }
        public int version_id { get; set; } 
        public int progressive { get; set; }
    }

    public class DictionaryDTO
    {
        public int? id { get; set; }
        public string value { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
        public int version_id { get; set; }
    }

    public class FiscalLocalizationDTO
    {
        public int? id { get; set; }
        public string value { get; set; }
        public string name { get; set; }
        public string name_en { get; set; }
        public int version_id { get; set; }
    }






    public class ModulePackageMagoRequestDTO
    {
        public List<ModulePackageMagoUpdateDTO> ModulesToAdd { get; set; }
        public List<ModulePackageMagoUpdateDTO> ModulesToUpdate { get; set; }
        public List<ModulePackageMagoUpdateDTO> ModulesToDelete { get; set; }
    }

    public class ModuleIndustryRequestDTO
    {
        public List<ModuleIndustryUpdateDTO> ModulesToAdd { get; set; }
        public List<ModuleIndustryUpdateDTO> ModulesToUpdate { get; set; }
        public List<ModuleIndustryUpdateDTO> ModulesToDelete { get; set; }
    }

}
