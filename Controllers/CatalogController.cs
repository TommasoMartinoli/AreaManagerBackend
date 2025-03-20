using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ADLoginAPI.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using ADLoginAPI.Models;
using ADLoginAPI.DTO;
using Serilog;
using System.Text.RegularExpressions;

//[Route("api/catalog")]
[ApiController]
public class CatalogController : ControllerBase
{
    private readonly CatalogDbContext _context;
    private readonly ILogger<CatalogController> _logger;

    public CatalogController(CatalogDbContext context, ILogger<CatalogController> logger)
    {
        _context = context;
        _logger = logger;
    }

    ////////////////EDITION

    [HttpGet("api/mago_editions")]
    public async Task<ActionResult<object>> GetMagoEditions()
    {
        try
        {
            var editions = await _context.mago_edition
               .Include(e => e.version)
               .ToListAsync();

            var response = new
            {
                magoVersions = editions
                    .Where(e => e.version_id == 1)
                    .Select(e => new
                    {
                        id = e.id,
                        name = e.name,
                        type = e.type,
                        code_type = e.code_type,
                        suffix = e.suffix
                    }).ToList(),

                magoCloudVersions = editions
                    .Where(e => e.version_id == 2)
                    .Select(e => new
                    {
                        id = e.id,
                        name = e.name,
                        type = e.type
                    }).ToList(),

                magoWebVersions = editions
                    .Where(e => e.version_id == 3)
                    .Select(e => new
                    {
                        id = e.id,
                        name = e.name,
                        type = e.type,
                        suffix = e.suffix
                    }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante il recupero delle edizioni Mago");
            return StatusCode(500, new { message = "Errore interno del server" });
        }
    }

    ////////////MODULE

    [HttpGet("api/module_main")]
    public async Task<ActionResult<IEnumerable<object>>> GetModules()
    {
        return await _context.module_main
              .OrderBy(m => m.name) 
              .Select(m => new ModuleDTO
              {
                  id = m.id,
                  name = m.name,
                  option_id = m.option_id,
                  short_description = m.short_description,
                  app_area = m.app_area,
                  available = m.available,
                  available_cloud = m.available_cloud,
                  available_web = m.available_web
              })
              .ToListAsync();
    }

    [HttpGet("api/module_main/all/{onlyAvailableModules}")]
    public async Task<ActionResult<IEnumerable<object>>> GetAllModules(int onlyAvailableModules)
    {
        var query = _context.module_main
            .Include(m => m.description_info)
            .Include(m => m.features)
            .AsQueryable(); 

        if (onlyAvailableModules == 1)
        {
            query = query.Where(m => m.available == 1 || m.available_cloud == 1 || m.available_web == 1);
        }

        var modules = await query.ToListAsync();

        foreach (var module in modules)
        {
            module.module_packages_mago = await _context.module_x_package_mago
                .OrderBy(f => f.progressive)
                .Where(mpw => mpw.module_id == module.id && mpw.status != 0)
                .ToListAsync();

            module.module_industries_cloud = await _context.module_x_industry_cloud
                .OrderBy(f => f.progressive)
                .Where(mpw => mpw.module_id == module.id && mpw.status != 0)
                .ToListAsync();

            module.module_packages_web = await _context.module_x_package_web
                .OrderBy(f => f.progressive)
                .Where(mpw => mpw.module_id == module.id && mpw.status != 0)
                .ToListAsync();
        }

        var response = modules.Select(module => new
        {
            id = module.id,
            name = module.name,
            available = Convert.ToBoolean(module.available),
            available_cloud = Convert.ToBoolean(module.available_cloud),
            available_web = Convert.ToBoolean(module.available_web),
            app_area = module.app_area,
            mago_allow_iso = module.mago_allow_iso.ToUpper(),
            mago_deny_iso = module.mago_deny_iso.ToUpper(),
            cloud_allow_iso = module.cloud_allow_iso.ToUpper(),
            cloud_deny_iso = module.cloud_deny_iso.ToUpper(),
            web_allow_iso = module.web_allow_iso.ToUpper(),
            web_deny_iso = module.web_deny_iso.ToUpper(),

            description_info = module.description_info.Select(d => new
            {
                id = d.id,
                module_id = d.module_id,
                title = d.title,
                description = d.description,
                language = d.language
            }).ToList(),

            features = module.features
                .OrderBy(f => f.progressive)
                .Select(f => new ModuleFeatureDTO
                {
                    id = f.id,
                    module_id = f.module_id,
                    description = f.description,
                    description_en = f.description_en,
                    progressive = f.progressive,
                    countries = f.countries,
                    deny_countries = f.deny_countries
                }).ToList(),

            packages_mago = module.module_packages_mago.Select(mp => new ModulePackageMagoDTO
            {
                edition_id = mp.edition_id,
                module_id = mp.module_id,
                package_id = mp.package_id,
                status = mp.status
            }).ToList(),

            industries_cloud = module.module_industries_cloud.Select(mi => new ModuleIndustryCloudDTO
            {
                edition_id = mi.edition_id,
                module_id = mi.module_id,
                industry_id = mi.industry_id,
                status = mi.status
            }).ToList(),

            packages_web = module.module_packages_web.Select(mpw => new ModulePackageWebDTO
            {
                edition_id = mpw.edition_id,
                module_id = mpw.module_id,
                package_id = mpw.package_id,
                status = mpw.status
            }).ToList(),

             mago_editions = _context.module_edition
            .Where(me => me.module_id == module.id && _context.mago_edition.Any(e => e.id == me.edition_id && e.version_id == 1))
            .Select(me => new
            {
                edition_id = me.edition_id,
                module_id = me.module_id,
                status = me.status
            })
            .ToList(),

            cloud_editions = _context.module_edition
            .Where(me => me.module_id == module.id && _context.mago_edition.Any(e => e.id == me.edition_id && e.version_id == 2))
            .Select(me => new
            {
                edition_id = me.edition_id,
                module_id = me.module_id,
                status = me.status
            })
            .ToList(),

            web_editions = _context.module_edition
            .Where(me => me.module_id == module.id && _context.mago_edition.Any(e => e.id == me.edition_id && e.version_id == 3))
            .Select(me => new
            {
                edition_id = me.edition_id,
                module_id = me.module_id,
                status = me.status
            })
            .ToList()

        }).ToList();

        return Ok(response);
    }

    [HttpGet("api/module_main/{id}")]
    public async Task<ActionResult<object>> GetModuleById(int id)
    {
        var module = await _context.module_main
        .Include(m => m.description_info)
        .Include(m => m.features)
        .FirstOrDefaultAsync(m => m.id == id);

        if (module == null)
            return NotFound(new { message = "Modulo non trovato" });

        var editions = await _context.mago_edition.ToListAsync();

        var moduleEditions = await _context.module_edition
            .Where(me => me.module_id == id)
            .ToListAsync();

        var magoEditionsDTO = moduleEditions
            .Where(me => editions.Any(e => e.id == me.edition_id && e.version_id == 1))
            .Select(me => new ModuleEditionDTO
            {
                edition_id = me.edition_id,
                module_id = me.module_id,
                status = me.status
            })
            .ToList();

        var magoCloudEditionsDTO = moduleEditions
            .Where(me => editions.Any(e => e.id == me.edition_id && e.version_id == 2))
            .Select(me => new ModuleEditionDTO
            {
                edition_id = me.edition_id,
                module_id = me.module_id,
                status = me.status
            })
            .ToList();

        var magoWebEditionsDTO = moduleEditions
            .Where(me => editions.Any(e => e.id == me.edition_id && e.version_id == 3))
            .Select(me => new ModuleEditionDTO
            {
                edition_id = me.edition_id,
                module_id = me.module_id,
                status = me.status
            })
            .ToList();

        var module_packages_mago = await _context.module_x_package_mago
               .OrderBy(f => f.progressive)
               .Where(mpw => mpw.module_id == id)
               .ToListAsync();

        var module_industries_cloud = await _context.module_x_industry_cloud
            .OrderBy(f => f.progressive)
            .Where(mpw => mpw.module_id == id)
            .ToListAsync();

        var module_packages_web = await _context.module_x_package_web
            .OrderBy(f => f.progressive)
            .Where(mpw => mpw.module_id == id)
            .ToListAsync();

        var response = new
        {
            id = module.id,
            name = module.name,
            short_description = module.short_description,
            folders_application = module.folders_application,
            folders_module = module.folders_module,
            option_id = module.option_id,
            cal_type = module.cal_type,
            available = Convert.ToBoolean(module.available),
            available_cloud = Convert.ToBoolean(module.available_cloud),
            available_web = Convert.ToBoolean(module.available_web),
            app_area = module.app_area,
            mago_code = module.mago_code,
            mago_cloud_code = module.mago_cloud_code,
            mago_web_code = module.mago_web_code,

            description_info = module.description_info.Select(d => new
                    {
                        id = d.id,
                        module_id = d.module_id,
                        title = d.title,
                        description = d.description,
                        language = d.language
                    }).ToList(),

            features = module.features
                    .OrderBy(f => f.progressive)
                    .Select(f => new ModuleFeatureDTO
                    {
                        id = f.id,
                        module_id = f.module_id,
                        description = f.description,
                        description_en = f.description_en,
                        progressive = f.progressive,
                        countries = f.countries,
                        deny_countries = f.deny_countries
                    }).ToList(),

            packages_mago = module_packages_mago.Select(mp => new ModulePackageMagoDTO
            {
                edition_id = mp.edition_id,
                module_id = mp.module_id,
                package_id = mp.package_id,
                status = mp.status
            }).ToList(),

            industries_cloud = module_industries_cloud.Select(mi => new ModuleIndustryCloudDTO
            {
                edition_id = mi.edition_id,
                module_id = mi.module_id,
                industry_id = mi.industry_id,
                status = mi.status
            }).ToList(),

            packages_web = module_packages_web.Select(mpw => new ModulePackageWebDTO
            {
                edition_id = mpw.edition_id,
                module_id = mpw.module_id,
                package_id = mpw.package_id,
                status = mpw.status
            }).ToList(),

            mago_rel_version = module.mago_rel_version,
            mago_valid_date = module.mago_valid_date?.ToString("yyyy-MM-dd") ?? "",
            mago_solution = module.mago_solution,
            mago_sales_module = module.mago_sales_module,
            mago_allow_iso = module.mago_allow_iso,
            mago_deny_iso = module.mago_deny_iso,
            mago_sales_module_dependency = module.mago_sales_module_dependency,
            mago_product_id = module.mago_product_id,
            mago_short_names = module.mago_short_names,
            mago_editions = magoEditionsDTO,


            cloud_rel_version = module.cloud_rel_version,
            cloud_valid_date = module.cloud_valid_date?.ToString("yyyy-MM-dd") ?? "",
            cloud_fragment_name = module.cloud_fragment_name,
            cloud_allow_iso = module.cloud_allow_iso,
            cloud_deny_iso = module.cloud_deny_iso,
            cloud_prerequisites = module.cloud_prerequisites,
            cloud_incompatibilities = module.cloud_incompatibilities,
            cloud_editions = magoCloudEditionsDTO,


            web_rel_version = module.web_rel_version,
            web_valid_date = module.web_valid_date?.ToString("yyyy-MM-dd") ?? "",
            web_fragment_name = module.web_fragment_name,
            web_allow_iso = module.web_allow_iso,
            web_deny_iso = module.web_deny_iso,
            web_prerequisites = module.web_prerequisites,
            web_incompatibilities = module.web_incompatibilities,
            web_editions = magoWebEditionsDTO,
        };

        return Ok(response);
    }

    [HttpPost("api/module_main")]
    public async Task<ActionResult<module_main>> CreateModule([FromBody] ModuleData moduleData)
    {
        if (moduleData == null)
            return BadRequest(new { message = "Dati non validi" });

        int maxProgressive = await _context.module_main
                            .Where(m => m.app_area == moduleData.main.app_area)
                            .MaxAsync(m => (int?)m.progressive) ?? 0;

        bool isCopiedModule  = false;

        if (moduleData.main.id == -1)
        {
            isCopiedModule = true;
        }

        var newModule = new module_main
        {
            name = moduleData.main.name,
            short_description = moduleData.main.short_description,
            folders_application = moduleData.main.folders_application,
            folders_module = moduleData.main.folders_module,
            option_id = moduleData.main.option_id,
            cal_type = moduleData.main.cal_type,
            available = (byte)(moduleData.main.available ? 1 : 0),
            available_cloud = (byte)(moduleData.main.available_cloud ? 1 : 0),
            available_web = (byte)(moduleData.main.available_web ? 1 : 0),
            app_area = moduleData.main.app_area,
            progressive = maxProgressive + 1,
            mago_code = moduleData.main.mago_code,
            mago_cloud_code = moduleData.main.mago_cloud_code,
            mago_web_code = moduleData.main.mago_web_code,

            mago_rel_version = moduleData.mago.mago_rel_version,
            mago_valid_date = moduleData.mago.mago_valid_date,
            mago_solution = moduleData.mago.mago_solution,
            mago_sales_module = moduleData.mago.mago_sales_module,
            mago_allow_iso = moduleData.mago.mago_allow_iso,
            mago_deny_iso = moduleData.mago.mago_deny_iso,
            mago_sales_module_dependency = moduleData.mago.mago_sales_module_dependency,
            mago_product_id = moduleData.mago.mago_product_id,
            mago_short_names = moduleData.mago.mago_short_names,

            cloud_rel_version = moduleData.magoCloud.cloud_rel_version,
            cloud_valid_date = moduleData.magoCloud.cloud_valid_date,
            cloud_fragment_name = moduleData.magoCloud.cloud_fragment_name,
            cloud_allow_iso = moduleData.magoCloud.cloud_allow_iso,
            cloud_deny_iso = moduleData.magoCloud.cloud_deny_iso,
            cloud_prerequisites = moduleData.magoCloud.cloud_prerequisites,
            cloud_incompatibilities = moduleData.magoCloud.cloud_incompatibilities,

            web_rel_version = moduleData.magoWeb.web_rel_version,
            web_valid_date = moduleData.magoWeb.web_valid_date,
            web_fragment_name = moduleData.magoWeb.web_fragment_name,
            web_allow_iso = moduleData.magoWeb.web_allow_iso,
            web_deny_iso = moduleData.magoWeb.web_deny_iso,
            web_prerequisites = moduleData.magoWeb.web_prerequisites,
            web_incompatibilities = moduleData.magoWeb.web_incompatibilities
        };

        _context.module_main.Add(newModule);
        await _context.SaveChangesAsync();

        if (moduleData.main.description_info != null && moduleData.main.description_info.Count > 0)
        {
            foreach (var desc in moduleData.main.description_info)
            {
                _context.module_description.Add(new module_description
                {
                    module_id = newModule.id, 
                    title = desc.title,
                    description = desc.description,
                    language = desc.language
                });
            }

            await _context.SaveChangesAsync(); 
        }

        if (moduleData.main.features != null && moduleData.main.features.Count > 0)
        {
            foreach (var feature in moduleData.main.features)
            {
                _context.module_feature.Add(new module_feature
                {
                    module_id = newModule.id,
                    description = feature.description,
                    description_en = feature.description_en,
                    progressive = feature.progressive,
                    countries = feature.countries,
                    deny_countries = feature.deny_countries
                });
            }

            await _context.SaveChangesAsync();
        }

        List<module_edition> moduleEditions = new List<module_edition>();

        if (moduleData.mago.mago_editions != null)
        {
            foreach (var edition in moduleData.mago.mago_editions)
            {
                moduleEditions.Add(new module_edition
                {
                    edition_id = edition.edition_id,
                    module_id = newModule.id,
                    status = edition.status
                });
            }
        }

        if (moduleData.magoCloud.cloud_editions != null)
        {
            foreach (var edition in moduleData.magoCloud.cloud_editions)
            {
                moduleEditions.Add(new module_edition
                {
                    edition_id = edition.edition_id,
                    module_id = newModule.id,
                    status = edition.status
                });
            }
        }

        if (moduleData.magoWeb.web_editions != null)
        {
            foreach (var edition in moduleData.magoWeb.web_editions)
            {
                moduleEditions.Add(new module_edition
                {
                    edition_id = edition.edition_id,
                    module_id = newModule.id,
                    status = edition.status
                });
            }
        }

        if (moduleEditions.Count > 0)
        {
            _context.module_edition.AddRange(moduleEditions);
            await _context.SaveChangesAsync();
        }

        if(isCopiedModule)
        {
            List<module_x_package_mago> modulePackages = new List<module_x_package_mago>();

            if (moduleData.mago.selected_packages != null)
            {
                foreach (var element in moduleData.mago.selected_packages)
                {
                    modulePackages.Add(new module_x_package_mago
                    {
                        module_id = newModule.id,
                        edition_id = element.edition_id,
                        package_id = element.package_id,
                        status = element.status,
                        progressive = element.progressive
                    });
                }
            }

            if (modulePackages.Count > 0)
            {
                _context.module_x_package_mago.AddRange(modulePackages);
                await _context.SaveChangesAsync();
            }

            List<module_x_industry_cloud> moduleIndustries = new List<module_x_industry_cloud>();

            if (moduleData.magoCloud.selected_industries != null)
            {
                foreach (var element in moduleData.magoCloud.selected_industries)
                {
                    moduleIndustries.Add(new module_x_industry_cloud
                    {
                        module_id = newModule.id,
                        edition_id = element.edition_id,
                        industry_id = element.industry_id,
                        status = element.status,
                        progressive = element.progressive
                    });
                }
            }

            if (moduleIndustries.Count > 0)
            {
                _context.module_x_industry_cloud.AddRange(moduleIndustries);
                await _context.SaveChangesAsync();
            }

            List<module_x_package_web> modulePackagesWeb = new List<module_x_package_web>();

            if (moduleData.magoWeb.selected_packages != null)
            {
                foreach (var element in moduleData.magoWeb.selected_packages)
                {
                    modulePackagesWeb.Add(new module_x_package_web
                    {
                        module_id = newModule.id,
                        edition_id = element.edition_id,
                        package_id = element.package_id,
                        status = element.status,
                        progressive = element.progressive
                    });
                }
            }

            if (modulePackagesWeb.Count > 0)
            {
                _context.module_x_package_web.AddRange(modulePackagesWeb);
                await _context.SaveChangesAsync();
            }
        }

        return Ok(new { id = newModule.id });
    }

    [HttpPut("api/module_main/{id}")]
    public async Task<IActionResult> UpdateModule(int id, [FromBody] ModuleData moduleData)
    {
        var existingModule = await _context.module_main
            .Include(m => m.description_info)
            .Include(m => m.features)
            .FirstOrDefaultAsync(m => m.id == id);

        if (existingModule == null)
            return NotFound(new { message = "Modulo non trovato" });

        int oldArea = existingModule.app_area;
        int oldProgressive = existingModule.progressive;
        int newArea = moduleData.main.app_area;

        if (oldArea != newArea)
        {
            await _context.module_main
                .Where(m => m.app_area == oldArea && m.progressive > oldProgressive)
                .ForEachAsync(m => m.progressive--);

            int newMaxProgressive = await _context.module_main
                .Where(m => m.app_area == newArea)
                .MaxAsync(m => (int?)m.progressive) ?? 0;

            existingModule.progressive = newMaxProgressive + 1; 
        }

        existingModule.name = moduleData.main.name;
        existingModule.short_description = moduleData.main.short_description;
        existingModule.folders_application = moduleData.main.folders_application;
        existingModule.folders_module = moduleData.main.folders_module;
        existingModule.option_id = moduleData.main.option_id;
        existingModule.cal_type = moduleData.main.cal_type;
        existingModule.available = (byte)(moduleData.main.available ? 1 : 0);
        existingModule.available_cloud = (byte)(moduleData.main.available_cloud ? 1 : 0);
        existingModule.available_web = (byte)(moduleData.main.available_web ? 1 : 0);
        existingModule.app_area = moduleData.main.app_area;
        existingModule.mago_code = moduleData.main.mago_code;
        existingModule.mago_cloud_code = moduleData.main.mago_cloud_code;
        existingModule.mago_web_code = moduleData.main.mago_web_code;

        existingModule.mago_rel_version = moduleData.mago.mago_rel_version;
        existingModule.mago_valid_date = moduleData.mago.mago_valid_date;
        existingModule.mago_solution = moduleData.mago.mago_solution;
        existingModule.mago_sales_module = moduleData.mago.mago_sales_module;
        existingModule.mago_allow_iso = moduleData.mago.mago_allow_iso;
        existingModule.mago_deny_iso = moduleData.mago.mago_deny_iso;
        existingModule.mago_sales_module_dependency = moduleData.mago.mago_sales_module_dependency;
        existingModule.mago_product_id = moduleData.mago.mago_product_id;
        existingModule.mago_short_names = moduleData.mago.mago_short_names;

        existingModule.cloud_rel_version = moduleData.magoCloud.cloud_rel_version;
        existingModule.cloud_valid_date = moduleData.magoCloud.cloud_valid_date;
        existingModule.cloud_fragment_name = moduleData.magoCloud.cloud_fragment_name;
        existingModule.cloud_allow_iso = moduleData.magoCloud.cloud_allow_iso;
        existingModule.cloud_deny_iso = moduleData.magoCloud.cloud_deny_iso;
        existingModule.cloud_prerequisites = moduleData.magoCloud.cloud_prerequisites;
        existingModule.cloud_incompatibilities = moduleData.magoCloud.cloud_incompatibilities;

        existingModule.web_rel_version = moduleData.magoWeb.web_rel_version;
        existingModule.web_valid_date = moduleData.magoWeb.web_valid_date;
        existingModule.web_fragment_name = moduleData.magoWeb.web_fragment_name;
        existingModule.web_allow_iso = moduleData.magoWeb.web_allow_iso;
        existingModule.web_deny_iso = moduleData.magoWeb.web_deny_iso;
        existingModule.web_prerequisites = moduleData.magoWeb.web_prerequisites;
        existingModule.web_incompatibilities = moduleData.magoWeb.web_incompatibilities;

        var updatedDescriptions = moduleData.main.description_info;

        var descriptionsToRemove = existingModule.description_info
            .Where(desc => !updatedDescriptions.Any(d => d.id == desc.id))
            .ToList();

        foreach (var desc in descriptionsToRemove)
        {
            _context.module_description.Remove(desc);
        }

        foreach (var desc in updatedDescriptions)
        {
            var existingDescription = existingModule.description_info.FirstOrDefault(d => d.id == desc.id);
            if (existingDescription != null)
            {
                existingDescription.title = desc.title;
                existingDescription.description = desc.description;
                existingDescription.language = desc.language;
            }
            else
            {
                _context.module_description.Add(new module_description
                {
                    module_id = id,
                    title = desc.title,
                    description = desc.description,
                    language = desc.language
                });
            }
        }

        var updatedFeatures = moduleData.main.features;

        var featuresToRemove = existingModule.features
            .Where(feat => !updatedFeatures.Any(f => f.id == feat.id))
            .ToList();

        foreach (var feat in featuresToRemove)
        {
            _context.module_feature.Remove(feat);
        }

        foreach (var feat in updatedFeatures)
        {
            var existingFeature = existingModule.features.FirstOrDefault(f => f.id == feat.id);
            if (existingFeature != null)
            {
                existingFeature.progressive = feat.progressive;
                existingFeature.countries = feat.countries;
                existingFeature.deny_countries = feat.deny_countries;
                existingFeature.description = feat.description;
                existingFeature.description_en = feat.description_en;
            }
            else
            {
                _context.module_feature.Add(new module_feature
                {
                    module_id = id,
                    progressive = feat.progressive,
                    countries = feat.countries,
                    deny_countries = feat.deny_countries,
                    description = feat.description,
                    description_en = feat.description_en
                });
            }
        }

        var existingModuleEditions = await _context.module_edition
            .Where(me => me.module_id == id)
            .ToListAsync();

        var updatedEditions = new List<ModuleEditionDTO>();

        if (moduleData.mago.mago_editions != null)
            updatedEditions.AddRange(moduleData.mago.mago_editions);

        if (moduleData.magoCloud.cloud_editions != null)
            updatedEditions.AddRange(moduleData.magoCloud.cloud_editions);

        if (moduleData.magoWeb.web_editions != null)
            updatedEditions.AddRange(moduleData.magoWeb.web_editions);

        var editionsToRemove = existingModuleEditions
            .Where(e => !updatedEditions.Any(ue => ue.edition_id == e.edition_id))
            .ToList();

        _context.module_edition.RemoveRange(editionsToRemove);

        foreach (var updatedEdition in updatedEditions)
        {
            var existingEdition = existingModuleEditions
                .FirstOrDefault(e => e.edition_id == updatedEdition.edition_id);

            if (existingEdition != null)
            {
                existingEdition.status = (byte)updatedEdition.status;
            }
            else
            {
                _context.module_edition.Add(new module_edition
                {
                    edition_id = updatedEdition.edition_id,
                    module_id = id,
                    status = (byte)updatedEdition.status
                });
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("api/module_main/{id}")]
    public async Task<IActionResult> DeleteModule(int id)
    {
        var module = await _context.module_main
            .Include(m => m.description_info)
            .Include(m => m.features)
            .Include(m => m.editions) 
            .FirstOrDefaultAsync(m => m.id == id);

        if (module == null)
        {
            return NotFound(new { message = "Modulo non trovato" });
        }

        int areaId = module.app_area;
        int oldProgressive = module.progressive;

        _context.module_description.RemoveRange(module.description_info);

        _context.module_feature.RemoveRange(module.features);

        _context.module_edition.RemoveRange(module.editions);

        _context.module_main.Remove(module);

        await _context.SaveChangesAsync();

        await _context.module_main
            .Where(m => m.app_area == areaId && m.progressive > oldProgressive)
            .ForEachAsync(m => m.progressive--);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Modulo eliminato con successo" });
    }

    ////////////MODULE MANAGEMENT

    [HttpGet("api/area_x_module")]
    public async Task<ActionResult<IEnumerable<object>>> GetAreasWithModules()
    {
        var areasWithModules = await _context.area
            .OrderBy(a => a.progressive)
            .Select(a => new
            {
                id = a.id,
                name = a.name,
                name_en = a.name_en,
                modules = _context.module_main
                    .Where(m => m.app_area == a.id)
                    .OrderBy(m => m.progressive) 
                    .Select(m => new
                    {
                        id = m.id,
                        name = m.name,
                        product_id = m.option_id,
                        progressive = m.progressive,
                        available = m.available,
                        available_cloud = m.available_cloud,
                        available_web = m.available_web,

                        mago_editions = _context.module_edition
                        .Where(me => me.module_id == m.id && _context.mago_edition.Any(e => e.id == me.edition_id && e.version_id == 1))
                        .Select(me => new
                        {
                            edition_id = me.edition_id,
                            module_id = me.module_id,
                            status = me.status
                        })
                        .ToList(),

                        cloud_editions = _context.module_edition
                        .Where(me => me.module_id == m.id && _context.mago_edition.Any(e => e.id == me.edition_id && e.version_id == 2))
                        .Select(me => new
                        {
                            edition_id = me.edition_id,
                            module_id = me.module_id,
                            status = me.status
                        })
                        .ToList(),

                        web_editions = _context.module_edition
                        .Where(me => me.module_id == m.id && _context.mago_edition.Any(e => e.id == me.edition_id && e.version_id == 3))
                        .Select(me => new
                        {
                            edition_id = me.edition_id,
                            module_id = me.module_id,
                            status = me.status
                        })
                        .ToList()
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(areasWithModules);
    }

    [HttpPut("api/update_modules_progressive")]
    public async Task<IActionResult> UpdateModulesProgressive([FromBody] List<AreaModulesDTO> areaModules)
    {
        if (areaModules == null || areaModules.Count == 0)
            return BadRequest(new { message = "Nessun dato ricevuto" });

        foreach (var area in areaModules)
        {
            foreach (var module in area.modules)
            {
                var existingModule = await _context.module_main.FirstOrDefaultAsync(m => m.id == module.id);

                if (existingModule != null)
                {
                    existingModule.progressive = module.progressive;
                    existingModule.app_area = area.id; 
                }
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { message = "Progressivi aggiornati con successo" });
    }

    [HttpGet("api/industry_x_module")]
    public async Task<ActionResult<IEnumerable<object>>> GetIndustriesWithModules()
    {
        var industries = await _context.industry
            .OrderBy(i => i.progressive)
            .ToListAsync();

        var modulesByIndustry = await _context.module_x_industry_cloud
            .Where(i => i.status != 0)
            .Join(_context.module_main,
                mic => mic.module_id,
                m => m.id,
                (mic, m) => new
                {
                    mic.industry_id,
                    mic.module_id,
                    mic.edition_id,
                    mic.status,
                    mic.progressive,
                    module_name = m.name,
                    available = m.available,
                    available_cloud = m.available_cloud,
                    available_web = m.available_web,
                    product_id = m.option_id
                })
            .ToListAsync();


        var industriesWithModules = industries.Select(i => new
        {
            id = i.id,
            name = i.name,
            modules = modulesByIndustry
        .Where(mic => mic.industry_id == i.id)
        .OrderBy(m => m.progressive)
        .ToList()
        }).ToList();

        return Ok(industriesWithModules);
    }

    [HttpGet("api/package_x_module_mago")]
    public async Task<ActionResult<IEnumerable<object>>> GetPackagesWithModulesMago()
    {
        var packages = await _context.package
            .OrderBy(i => i.progressive)
            .ToListAsync();

        var modulesByPackage = await _context.module_x_package_mago
            .Where(i => i.status != 0)
            .Join(_context.module_main,
                mic => mic.module_id,
                m => m.id,
                (mic, m) => new
                {
                    mic.package_id,
                    mic.module_id,
                    mic.edition_id,
                    mic.status,
                    mic.progressive,
                    module_name = m.name,
                    available = m.available,
                    product_id = m.option_id
                })
            .ToListAsync();

        var packagesWithModules = packages.Select(i => new
        {
            id = i.id,
            name = i.name,
            modules = modulesByPackage
        .Where(mic => mic.package_id == i.id)
        .OrderBy(m => m.progressive)
        .ToList()
        }).ToList();

        return Ok(packagesWithModules);
    }

    [HttpGet("api/package_x_module_web")]
    public async Task<ActionResult<IEnumerable<object>>> GetPackagesWithModulesWeb()
    {
        var packages = await _context.package_web
            .OrderBy(i => i.progressive)
            .ToListAsync();

        var modulesByPackage = await _context.module_x_package_web
            .Where(i => i.status != 0)
            .Join(_context.module_main,
                mic => mic.module_id,
                m => m.id,
                (mic, m) => new
                {
                    mic.package_id,
                    mic.module_id,
                    mic.edition_id,
                    mic.status,
                    mic.progressive,
                    module_name = m.name,
                    available = m.available,
                    available_cloud = m.available_cloud,
                    available_web = m.available_web,
                    product_id = m.option_id
                })
            .ToListAsync();

        var packagesWithModules = packages.Select(i => new
        {
            id = i.id,
            name = i.name,
            modules = modulesByPackage
        .Where(mic => mic.package_id == i.id)
        .OrderBy(m => m.progressive)
        .ToList()
        }).ToList();

        return Ok(packagesWithModules);
    }

    [HttpGet("api/get_package_x_module_mago")]
    public async Task<ActionResult<IEnumerable<object>>> GetPackagesWithModulesMagoNew()
    {
        var packages = await _context.package
            .OrderBy(i => i.progressive)
            .ToListAsync();

        var modulesByPackage = await _context.module_x_package_mago
            .Join(_context.module_main,
                mic => mic.module_id,
                m => m.id,
                (mic, m) => new
                {
                    mic.package_id,
                    mic.module_id,
                    mic.edition_id,
                    mic.status,
                    mic.progressive,
                    module_name = m.name
                })
            .ToListAsync();

        var groupedModules = modulesByPackage
            .GroupBy(m => new { m.module_id, m.progressive, m.package_id, m.module_name })
            .Select(g => new
            {
                module_id = g.Key.module_id,
                progressive = g.Key.progressive,
                package_id = g.Key.package_id,
                module_name = g.Key.module_name,
                editions = g.Select(e => new
                {
                    edition_id = e.edition_id,
                    status = e.status
                }).ToList()
            })
            .ToList();

        var packagesWithModules = packages.Select(p => new
        {
            id = p.id,
            name = p.name,
            modules = groupedModules.Where(m => m.package_id == p.id).OrderBy(m => m.progressive).ToList()
        }).ToList();

        return Ok(packagesWithModules);
    }

    [HttpGet("api/get_package_x_module_web")]
    public async Task<ActionResult<IEnumerable<object>>> GetPackagesWithModulesWebNew()
    {
        var packages = await _context.package_web
            .OrderBy(i => i.progressive)
            .ToListAsync();

        var modulesByPackage = await _context.module_x_package_web
            .Join(_context.module_main,
                mic => mic.module_id,
                m => m.id,
                (mic, m) => new
                {
                    mic.package_id,
                    mic.module_id,
                    mic.edition_id,
                    mic.status,
                    mic.progressive,
                    module_name = m.name
                })
            .ToListAsync();

        var groupedModules = modulesByPackage
            .GroupBy(m => new { m.module_id, m.progressive, m.package_id, m.module_name })
            .Select(g => new
            {
                module_id = g.Key.module_id,
                progressive = g.Key.progressive,
                package_id = g.Key.package_id,
                module_name = g.Key.module_name,
                editions = g.Select(e => new
                {
                    edition_id = e.edition_id,
                    status = e.status
                }).ToList()
            })
            .ToList();

        var packagesWithModules = packages.Select(p => new
        {
            id = p.id,
            name = p.name,
            modules = groupedModules.Where(m => m.package_id == p.id).OrderBy(m => m.progressive).ToList()
        }).ToList();

        return Ok(packagesWithModules);
    }

    [HttpGet("api/get_industry_x_module")]
    public async Task<ActionResult<IEnumerable<object>>> GetIndustriesWithModulesNew()
    {
        var industries = await _context.industry
            .OrderBy(i => i.progressive)
            .ToListAsync();

        var modulesByIndustry = await _context.module_x_industry_cloud
            .Join(_context.module_main,
                mic => mic.module_id,
                m => m.id,
                (mic, m) => new
                {
                    mic.industry_id,
                    mic.module_id,
                    mic.edition_id,
                    mic.status,
                    mic.progressive,
                    module_name = m.name
                })
            .ToListAsync();

        var groupedModules = modulesByIndustry
            .GroupBy(m => new { m.module_id, m.progressive, m.industry_id, m.module_name })
            .Select(g => new
            {
                module_id = g.Key.module_id,
                progressive = g.Key.progressive,
                industry_id = g.Key.industry_id,
                module_name = g.Key.module_name,
                editions = g.Select(e => new
                {
                    edition_id = e.edition_id,
                    status = e.status
                }).ToList()
            })
            .ToList();

        var industriesWithModules = industries.Select(p => new
        {
            id = p.id,
            name = p.name,
            modules = groupedModules.Where(m => m.industry_id == p.id).OrderBy(m => m.progressive).ToList()
        }).ToList();

        return Ok(industriesWithModules); 
    }

    [HttpPut("api/manage_package_x_module_mago")]
    public async Task<IActionResult> ManagePackageModulesMago(
    [FromBody] ModulePackageMagoRequestDTO request)
    {
        if (request == null) return BadRequest(new { message = "Dati mancanti." });

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (request.ModulesToDelete?.Any() == true)
            {
                var uniqueDeletions = request.ModulesToDelete
                    .GroupBy(d => new { d.package_id, d.module_id })
                    .Select(g => g.First())
                    .ToList();

                var allEntities = await _context.module_x_package_mago.ToListAsync();

                var entitiesToDelete = allEntities
                    .Where(m => uniqueDeletions.Any(d =>
                        d.package_id == m.package_id &&
                        d.module_id == m.module_id))
                    .ToList();

                _context.module_x_package_mago.RemoveRange(entitiesToDelete);
                await _context.SaveChangesAsync();
            }


            if (request.ModulesToUpdate?.Any() == true)
            {
                foreach (var module in request.ModulesToUpdate)
                {
                    var entity = await _context.module_x_package_mago.FirstOrDefaultAsync(m =>
                        m.package_id == module.package_id &&
                        m.module_id == module.module_id &&
                        m.edition_id == module.edition_id);

                    if (entity != null)
                    {
                        entity.progressive = module.progressive;
                        entity.status = module.status;
                        _context.module_x_package_mago.Update(entity);
                    }
                }
                await _context.SaveChangesAsync();
            }

            if (request.ModulesToAdd?.Any() == true)
            {
                var entitiesToAdd = request.ModulesToAdd.Select(dto => new module_x_package_mago
                {
                    package_id = dto.package_id,
                    module_id = dto.module_id,
                    edition_id = dto.edition_id,
                    status = dto.status,
                    progressive = dto.progressive
                }).ToList();

                _context.module_x_package_mago.AddRange(entitiesToAdd);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return Ok(new { message = "Operazione completata con successo." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Errore interno.", error = ex.Message });
        }
    }

    [HttpPut("api/manage_industry_x_module")]
    public async Task<IActionResult> ManageIndustryModules(
    [FromBody] ModuleIndustryRequestDTO request)
    {
        if (request == null) return BadRequest(new { message = "Dati mancanti." });

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (request.ModulesToDelete?.Any() == true)
            {
                var uniqueDeletions = request.ModulesToDelete
                    .GroupBy(d => new { d.industry_id, d.module_id })
                    .Select(g => g.First()) 
                    .ToList();

                var allEntities = await _context.module_x_industry_cloud.ToListAsync();

                var entitiesToDelete = allEntities
                    .Where(m => uniqueDeletions.Any(d =>
                        d.industry_id == m.industry_id &&
                        d.module_id == m.module_id))
                    .ToList();

                _context.module_x_industry_cloud.RemoveRange(entitiesToDelete);
                await _context.SaveChangesAsync();
            }

            if (request.ModulesToUpdate?.Any() == true)
            {
                foreach (var module in request.ModulesToUpdate)
                {
                    var entity = await _context.module_x_industry_cloud.FirstOrDefaultAsync(m =>
                        m.industry_id == module.industry_id &&
                        m.module_id == module.module_id &&
                        m.edition_id == module.edition_id);

                    if (entity != null)
                    {
                        entity.progressive = module.progressive;
                        entity.status = module.status;
                        _context.module_x_industry_cloud.Update(entity);
                    }
                }
                await _context.SaveChangesAsync();
            }

            if (request.ModulesToAdd?.Any() == true)
            {
                var entitiesToAdd = request.ModulesToAdd.Select(dto => new module_x_industry_cloud
                {
                    industry_id = dto.industry_id,
                    module_id = dto.module_id,
                    edition_id = dto.edition_id,
                    status = dto.status,
                    progressive = dto.progressive
                }).ToList();

                _context.module_x_industry_cloud.AddRange(entitiesToAdd);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return Ok(new { message = "Operazione completata con successo." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Errore interno.", error = ex.Message });
        }
    }

    [HttpPut("api/manage_package_x_module_web")]
    public async Task<IActionResult> ManagePackageModulesWeb(
    [FromBody] ModulePackageMagoRequestDTO request)
    {
        if (request == null) return BadRequest(new { message = "Dati mancanti." });

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            if (request.ModulesToDelete?.Any() == true)
            {
                var uniqueDeletions = request.ModulesToDelete
                    .GroupBy(d => new { d.package_id, d.module_id })
                    .Select(g => g.First())
                    .ToList();

                var allEntities = await _context.module_x_package_web.ToListAsync();

                var entitiesToDelete = allEntities
                    .Where(m => uniqueDeletions.Any(d =>
                        d.package_id == m.package_id &&
                        d.module_id == m.module_id))
                    .ToList();

                _context.module_x_package_web.RemoveRange(entitiesToDelete);
                await _context.SaveChangesAsync();
            }


            if (request.ModulesToUpdate?.Any() == true)
            {
                foreach (var module in request.ModulesToUpdate)
                {
                    var entity = await _context.module_x_package_web.FirstOrDefaultAsync(m =>
                        m.package_id == module.package_id &&
                        m.module_id == module.module_id &&
                        m.edition_id == module.edition_id);

                    if (entity != null)
                    {
                        entity.progressive = module.progressive;
                        entity.status = module.status;
                        _context.module_x_package_web.Update(entity);
                    }
                }
                await _context.SaveChangesAsync();
            }

            if (request.ModulesToAdd?.Any() == true)
            {
                var entitiesToAdd = request.ModulesToAdd.Select(dto => new module_x_package_web
                {
                    package_id = dto.package_id,
                    module_id = dto.module_id,
                    edition_id = dto.edition_id,
                    status = dto.status,
                    progressive = dto.progressive
                }).ToList();

                _context.module_x_package_web.AddRange(entitiesToAdd);
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            return Ok(new { message = "Operazione completata con successo." });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { message = "Errore interno.", error = ex.Message });
        }
    }

    ////////////TABLES

    [HttpGet("api/dictionary_available")]
    public async Task<ActionResult<object>> GetDictionaries()
    {
        var dictionaries = await _context.dictionary_available.ToListAsync();

        var result = new
        {
            mago = dictionaries.Where(d => d.version_id == 1).OrderBy(d => d.value).ToList(),
            magoCloud = dictionaries.Where(d => d.version_id == 2).OrderBy(d => d.value).ToList(),
            magoWeb = dictionaries.Where(d => d.version_id == 3).OrderBy(d => d.value).ToList()
        };

        return Ok(result);
    }

    [HttpGet("api/dictionary_available/{id}")]
    public async Task<ActionResult<dictionary_available>> GetDictionaryById(int id)
    {
        var dictionary_available = await _context.dictionary_available.FindAsync(id);
        if (dictionary_available == null) return NotFound();
        return dictionary_available;
    }

    [HttpPost("api/dictionary_available")]
    public async Task<ActionResult<dictionary_available>> CreateDictionary(dictionary_available dictionary_available)
    {
        _context.dictionary_available.Add(dictionary_available);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDictionaryById), new { id = dictionary_available.id }, dictionary_available);
    }

    [HttpPut("api/dictionary_available/{id}")]
    public async Task<IActionResult> UpdateDictionary(int id, dictionary_available dictionary_available)
    {
        if (id != dictionary_available.id) return BadRequest();

        _context.Entry(dictionary_available).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("api/dictionary_available/{id}")]
    public async Task<IActionResult> DeleteDictionary(int id)
    {
        var dictionary_available = await _context.dictionary_available.FindAsync(id);
        if (dictionary_available == null) return NotFound(new { message = "Dictionary non trovato." });

        try
        {
            _context.dictionary_available.Remove(dictionary_available);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new
            {
                message = "Ipossibile eliminare il dictionary.",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [HttpGet("api/fiscal_localization")]
    public async Task<ActionResult<IEnumerable<fiscal_localization>>> GetLocalization()
    {
        var localizations = await _context.fiscal_localization.ToListAsync();

        var result = new
        {
            mago = localizations.Where(d => d.version_id == 1).OrderBy(d => d.value).ToList(),
            magoCloud = localizations.Where(d => d.version_id == 2).OrderBy(d => d.value).ToList(),
            magoWeb = localizations.Where(d => d.version_id == 3).OrderBy(d => d.value).ToList()
        };

        return Ok(result);
    }

    [HttpGet("api/fiscal_localization/{id}")]
    public async Task<ActionResult<fiscal_localization>> GetLocalizationById(int id)
    {
        var fiscal_localization = await _context.fiscal_localization.FindAsync(id);
        if (fiscal_localization == null) return NotFound();
        return fiscal_localization;
    }

    [HttpPost("api/fiscal_localization")]
    public async Task<ActionResult<fiscal_localization>> CreateLocalization(fiscal_localization fiscal_localization)
    {
        _context.fiscal_localization.Add(fiscal_localization);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLocalizationById), new { id = fiscal_localization.id }, fiscal_localization);
    }

    [HttpPut("api/fiscal_localization/{id}")]
    public async Task<IActionResult> UpdateDictionary(int id, fiscal_localization fiscal_localization)
    {
        if (id != fiscal_localization.id) return BadRequest();

        _context.Entry(fiscal_localization).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("api/fiscal_localization/{id}")]
    public async Task<IActionResult> DeleteLocalization(int id)
    {
        var fiscal_localization = await _context.fiscal_localization.FindAsync(id);
        if (fiscal_localization == null) return NotFound(new { message = "Localizzazone non trovata." });

        try
        {
            _context.fiscal_localization.Remove(fiscal_localization);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new
            {
                message = "Ipossibile eliminare la localizzazione.",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [HttpGet("api/area")]
    public async Task<ActionResult<IEnumerable<area>>> GetAreas()
    {
        return await _context.area.ToListAsync();
    }

    [HttpGet("api/area/{id}")]
    public async Task<ActionResult<area>> GetAreaById(int id)
    {
        var area = await _context.area.FindAsync(id);
        if (area == null) return NotFound();
        return area;
    }

    [HttpPost("api/area")]
    public async Task<ActionResult<area>> CreateArea(area area)
    {
        _context.area.Add(area);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAreaById), new { id = area.id }, area);
    }

    [HttpPut("api/area/{id}")]
    public async Task<IActionResult> UpdateArea(int id, area area)
    {
        if (id != area.id) return BadRequest();

        _context.Entry(area).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("api/area/{id}")]
    public async Task<IActionResult> DeleteArea(int id)
    {
        var area = await _context.area.FindAsync(id);
        if (area == null) return NotFound(new { message = "Area non trovata." });

        try
        {
            _context.area.Remove(area);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new
            {
                message = "Ipossibile eliminare l'area.",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [HttpGet("api/industry")]
    public async Task<ActionResult<IEnumerable<industry>>> GetIndustries()
    {
        return await _context.industry.ToListAsync();
    }

    [HttpGet("api/industry/{id}")]
    public async Task<ActionResult<industry>> GetIndustryById(int id)
    {
        var industry = await _context.industry.FindAsync(id);
        if (industry == null) return NotFound();
        return industry;
    }

    [HttpPost("api/industry")]
    public async Task<ActionResult<industry>> CreateIndustry(industry industry)
    {
        _context.industry.Add(industry);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetIndustryById), new { id = industry.id }, industry);
    }

    [HttpPut("api/industry/{id}")]
    public async Task<IActionResult> UpdateIndustry(int id, industry industry)
    {
        if (id != industry.id) return BadRequest();

        _context.Entry(industry).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("api/industry/{id}")]
    public async Task<IActionResult> DeleteIndustry(int id)
    {
        var industry = await _context.industry.FindAsync(id);
        if (industry == null) return NotFound(new { message = "Industry non trovata." });

        try
        {
            _context.industry.Remove(industry);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new
            {
                message = "Impossibile eliminare la industry.",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [HttpGet("api/package")]
    public async Task<ActionResult<IEnumerable<package>>> GetPackages()
    {
        return await _context.package.ToListAsync();
    }

    [HttpGet("api/package/{id}")]
    public async Task<ActionResult<package>> GetPackageById(int id)
    {
        var package = await _context.package.FindAsync(id);
        if (package == null) return NotFound();
        return package;
    }

    [HttpPost("api/package")]
    public async Task<ActionResult<package>> CreatePackage(package package)
    {
        _context.package.Add(package);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPackageById), new { id = package.id }, package);
    }

    [HttpPut("api/package/{id}")]
    public async Task<IActionResult> UpdatePackage(int id, package package)
    {
        if (id != package.id) return BadRequest();

        _context.Entry(package).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("api/package/{id}")]
    public async Task<IActionResult> DeletePackage(int id)
    {
        var package = await _context.package.FindAsync(id);
        if (package == null) return NotFound(new { message = "Package non trovato." });

        try
        {
            _context.package.Remove(package);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new
            {
                message = "Impossibile eliminare il package.",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [HttpGet("api/package_web")]
    public async Task<ActionResult<IEnumerable<package_web>>> GetPackagesWeb()
    {
        return await _context.package_web.ToListAsync();
    }

    [HttpGet("api/package_web/{id}")]
    public async Task<ActionResult<package_web>> GetPackageWebById(int id)
    {
        var packageWeb = await _context.package_web.FindAsync(id);
        if (packageWeb == null) return NotFound();
        return packageWeb;
    }

    [HttpPost("api/package_web")]
    public async Task<ActionResult<package_web>> CreatePackageWeb(package_web packageWeb)
    {
        _context.package_web.Add(packageWeb);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetPackageWebById), new { id = packageWeb.id }, packageWeb);
    }

    [HttpPut("api/package_web/{id}")]
    public async Task<IActionResult> UpdatePackageWeb(int id, package_web packageWeb)
    {
        if (id != packageWeb.id) return BadRequest();

        _context.Entry(packageWeb).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("api/package_web/{id}")]
    public async Task<IActionResult> DeletePackageWeb(int id)
    {
        var packageWeb = await _context.package_web.FindAsync(id);
        if (packageWeb == null) return NotFound(new { message = "Package web non trovato." });

        try
        {
            _context.package_web.Remove(packageWeb);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new
            {
                message = "Impossibile eliminare il package web.",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    ////////////PARAGRAPH

    [HttpGet("api/paragraph")]
    public async Task<ActionResult<object>> GetParagraphs()
    {
        var paragraphs = await _context.paragraph
            .OrderBy(p => p.progressive)
            .ToListAsync();

        var groupedParagraphs = new
        {
            mago = paragraphs
                .Where(p => p.version_id == 1)
                .Select(p => new
                {
                    id = p.id,
                    version_id = p.version_id,
                    type = p.type,
                    title = p.title,
                    title_en = p.title_en,
                    content = p.content,
                    progressive = p.progressive
                })
                .ToList(),

            magoCloud = paragraphs
                .Where(p => p.version_id == 2)
                .Select(p => new
                {
                    id = p.id,
                    version_id = p.version_id,
                    type = p.type,
                    title = p.title,
                    title_en = p.title_en,
                    content = p.content,
                    progressive = p.progressive
                })
                .ToList(),

            magoWeb = paragraphs
                .Where(p => p.version_id == 3)
                .Select(p => new
                {
                    id = p.id,
                    version_id = p.version_id,
                    type = p.type,
                    title = p.title,
                    title_en = p.title_en,
                    content = p.content,
                    progressive = p.progressive
                })
                .ToList()
        };

        return Ok(groupedParagraphs);
    }


    [HttpGet("api/paragraph/{id}")]
    public async Task<ActionResult<paragraph>> GetSingleParagraph(int id)
    {
        var paragraph = await _context.paragraph.FindAsync(id);

        if (paragraph == null)
            return NotFound(new { message = "Paragrafo non trovato" });

        return paragraph;
    }

    [HttpPost("api/paragraph")]
    public async Task<ActionResult<paragraph>> CreateParagraph([FromBody] ParagraphDTO dto)
    {
        if (dto == null)
        {
            return BadRequest(new { message = "Dati del paragrafo non validi." });
        }

        var newParagraph = new paragraph
        {
            type = dto.type,
            title = dto.title,
            title_en = dto.title_en,
            content = dto.content,
            version_id = dto.version_id,
            progressive = dto.progressive
        };

        _context.paragraph.Add(newParagraph);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetSingleParagraph), new { id = newParagraph.id }, newParagraph);
    }

    [HttpPut("api/paragraph/{id}")]
    public async Task<IActionResult> UpdateParagraph(int id, [FromBody] ParagraphDTO dto)
    {
        if (dto == null || id != dto.id)
            return BadRequest(new { message = "ID non corrispondente o dati non validi" });

        var existingParagraph = await _context.paragraph.FindAsync(id);
        if (existingParagraph == null)
            return NotFound(new { message = "Paragrafo non trovato" });

        existingParagraph.version_id = dto.version_id;
        existingParagraph.type = dto.type;
        existingParagraph.title = dto.title;
        existingParagraph.title_en = dto.title_en;
        existingParagraph.content = dto.content;
        existingParagraph.progressive = dto.progressive;

        await _context.SaveChangesAsync();
        return NoContent();
    }


    [HttpDelete("api/paragraph/{id}")]
    public async Task<IActionResult> DeleteParagraph(int id)
    {
        var paragraph = await _context.paragraph.FindAsync(id);
        if (paragraph == null)
            return NotFound(new { message = "Paragrafo non trovato" });

        _context.paragraph.Remove(paragraph);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}