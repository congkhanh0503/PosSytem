using DiroPos.Api.Data;
using DiroPos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiroPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ServiceCategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceCategory>>> GetCategories()
    {
        return await _context.ServiceCategories
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<ServiceCategory>> CreateCategory([FromBody] ServiceCategory category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return BadRequest("Tên phân loại không được để trống.");
        }

        category.Name = category.Name.Trim();
        if (string.IsNullOrWhiteSpace(category.Color))
        {
            category.Color = "#f59e0b";
        }

        bool exists = await _context.ServiceCategories
            .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());
        if (exists)
        {
            return BadRequest("Phân loại dịch vụ này đã tồn tại.");
        }

        category.CreatedAt = DateTime.UtcNow;
        _context.ServiceCategories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.ServiceCategories.FindAsync(id);
        if (category == null) return NotFound();

        // Kiểm tra xem có dịch vụ nào đang dùng phân loại này không
        bool inUse = await _context.Services.AnyAsync(s => s.Category == category.Name);
        if (inUse)
        {
            return BadRequest($"Không thể xóa phân loại '{category.Name}' vì đang có dịch vụ sử dụng phân loại này. Vui lòng chuyển các dịch vụ sang phân loại khác trước.");
        }

        _context.ServiceCategories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
