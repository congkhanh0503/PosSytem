using DiroPos.Api.Data;
using DiroPos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiroPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServicesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServicesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceItem>>> GetServices([FromQuery] bool onlyActive = false)
    {
        var query = _context.Services.AsQueryable();
        if (onlyActive)
        {
            query = query.Where(s => s.IsActive);
        }
        return await query.OrderBy(s => s.Category).ThenBy(s => s.Price).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceItem>> GetService(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound();
        return service;
    }

    [HttpPost]
    public async Task<ActionResult<ServiceItem>> CreateService(ServiceItem service)
    {
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetService), new { id = service.Id }, service);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateService(int id, ServiceItem service)
    {
        if (id != service.Id) return BadRequest();

        _context.Entry(service).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Services.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null) return NotFound();

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
