using DiroPos.Api.Data;
using DiroPos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiroPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductCategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductCategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductCategory>>> GetCategories()
    {
        return await _context.ProductCategories
            .OrderBy(c => c.Id)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<ProductCategory>> CreateCategory([FromBody] ProductCategory category)
    {
        if (string.IsNullOrWhiteSpace(category.Name))
        {
            return BadRequest("Tên phân loại sản phẩm không được để trống.");
        }

        category.Name = category.Name.Trim();
        if (string.IsNullOrWhiteSpace(category.Color))
        {
            category.Color = "#06b6d4";
        }

        bool exists = await _context.ProductCategories
            .AnyAsync(c => c.Name.ToLower() == category.Name.ToLower());
        if (exists)
        {
            return BadRequest("Phân loại sản phẩm này đã tồn tại.");
        }

        category.CreatedAt = DateTime.UtcNow;
        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.ProductCategories.FindAsync(id);
        if (category == null) return NotFound();

        // Kiểm tra xem có sản phẩm nào đang dùng phân loại này không
        bool inUse = await _context.Products.AnyAsync(p => p.Category == category.Name);
        if (inUse)
        {
            return BadRequest($"Không thể xóa phân loại '{category.Name}' vì đang có sản phẩm trong kho thuộc phân loại này. Vui lòng chuyển các sản phẩm sang phân loại khác trước.");
        }

        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
