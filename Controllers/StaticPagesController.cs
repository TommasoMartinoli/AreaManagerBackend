using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ADLoginAPI.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using ADLoginAPI.Models;
using ADLoginAPI.DTO;
using Serilog;
using System.Text.RegularExpressions;
using System;

//[Route("api/static-pages")]
[ApiController]
public class StaticPagesController : ControllerBase
{
    private readonly CatalogDbContext _context;
    private readonly ILogger<CatalogController> _logger;

    public StaticPagesController(CatalogDbContext context, ILogger<CatalogController> logger)
    {
        _context = context;
        _logger = logger;
    }

    ////////////PAGE

    [HttpGet("api/html_page")]
    public async Task<ActionResult<IEnumerable<PageDTO>>> GetHtmlPages()
    {
        return await _context.html_page
             .OrderBy(p => p.title)
             .Select(p => new PageDTO
             {
                 id = p.id,
                 title = p.title,
                 description = p.description,
                 content = "",
                 category_id = p.category_id,
                 creation_date = p.creation_date
             })
             .ToListAsync();
    }

    [HttpGet("api/html_page/{id}")]
    public async Task<ActionResult<PageDTO>> GetHtmlPageById(int id)
    {
        var html_page = await _context.html_page
            .Where(p => p.id == id)
            .Select(p => new PageDTO
            {
                id = p.id,
                title = p.title,
                description = p.description,
                content = p.content,
                category_id = p.category_id,
                creation_date = p.creation_date
            })
            .FirstOrDefaultAsync();

        if (html_page == null) return NotFound();
        return html_page;
    }

    [HttpPost("api/html_page")]
    public async Task<ActionResult<html_page>> CreateHtmlPage(PageDTO dto)
    {
        var page = new html_page
        {
            title = dto.title,
            description = dto.description,
            content = dto.content,
            category_id = dto.category_id,
            creation_date = dto.creation_date ?? DateTime.Now
        };

        _context.html_page.Add(page);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHtmlPageById), new { id = page.id }, page);
    }

    [HttpPut("api/html_page/{id}")]
    public async Task<IActionResult> UpdateHtmlPage(int id, PageDTO pageDto)
    {
        var existing = await _context.html_page.FindAsync(id);
        if (existing == null) return NotFound();

        existing.title = pageDto.title;
        existing.description = pageDto.description;
        existing.content = pageDto.content;
        existing.creation_date = pageDto.creation_date;
        existing.category_id = pageDto.category_id;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("api/html_page/{id}")]
    public async Task<IActionResult> DeleteHtmlPage(int id)
    {
        var html_page = await _context.html_page.FindAsync(id);
        if (html_page == null) return NotFound(new { message = "Pagina non trovata." });

        try
        {
            _context.html_page.Remove(html_page);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new
            {
                message = "Impossibile eliminare la pagina.",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    ////////////CATEGORY

    [HttpGet("api/html_category")]
    public async Task<ActionResult<IEnumerable<html_category>>> GetHtmlCategory()
    {
        return await _context.html_category.OrderBy(p => p.name).ToListAsync();
    }

    [HttpGet("api/html_category/{id}")]
    public async Task<ActionResult<html_category>> GetHtmlCategoryById(int id)
    {
        var html_category = await _context.html_category.FindAsync(id);
        if (html_category == null) return NotFound();
        return html_category;
    }

    [HttpPost("api/html_category")]
    public async Task<ActionResult<html_category>> CreateArea(html_category html_category)
    {
        _context.html_category.Add(html_category);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetHtmlCategoryById), new { id = html_category.id }, html_category);
    }

    [HttpPut("api/html_category/{id}")]
    public async Task<IActionResult> UpdateHtmlCategory(int id, html_category html_category)
    {
        if (id != html_category.id) return BadRequest();

        _context.Entry(html_category).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("api/html_category/{id}")]
    public async Task<IActionResult> DeleteHtmlCategory(int id)
    {
        var html_category = await _context.html_category.FindAsync(id);
        if (html_category == null) return NotFound(new { message = "Categoria non trovata." });

        try
        {
            _context.html_category.Remove(html_category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException ex)
        {
            return BadRequest(new
            {
                message = "Impossibile eliminare la categoria.",
                details = ex.InnerException?.Message ?? ex.Message
            });
        }
    }

    [HttpGet("api/html_categories_pages")]
    public async Task<ActionResult<List<CategoryWithPagesDTO>>> GetCategoriesWithPages()
    {
        var result = await _context.html_category
            .OrderBy(c => c.name)
            .Select(cat => new CategoryWithPagesDTO
            {
                id = cat.id,
                name = cat.name,
                pages = _context.html_page
                          .Where(p => p.category_id == cat.id)
                          .OrderBy(p => p.title)
                          .Select(p => new PageDTO
                          {
                              id = p.id,
                              title = p.title,
                              description = p.description,
                              content = null,
                              category_id = p.category_id,
                              creation_date = p.creation_date
                          }).ToList()
            }).ToListAsync();

        return Ok(result);
    }

}