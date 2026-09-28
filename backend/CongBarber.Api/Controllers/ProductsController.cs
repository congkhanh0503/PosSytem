using CongBarber.Api.Data;
using CongBarber.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CongBarber.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProductsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductItem>>> GetProducts(
        [FromQuery] bool onlyActive = false,
        [FromQuery] bool onlyPos = false)
    {
        var query = _context.Products.AsQueryable();
        if (onlyActive)
        {
            query = query.Where(p => p.IsActive);
        }
        if (onlyPos)
        {
            query = query.Where(p => p.ShowOnPos);
        }
        return await query.OrderBy(p => p.Category).ThenBy(p => p.Name).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductItem>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        return product;
    }

    [HttpPost]
    public async Task<ActionResult<ProductItem>> CreateProduct(ProductItem product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, ProductItem product)
    {
        if (id != product.Id) return BadRequest();

        _context.Entry(product).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Products.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/stock")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] int newStock)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.StockQuantity = newStock;
        await _context.SaveChangesAsync();
        return Ok(product);
    }

    [HttpPatch("{id}/toggle-pos")]
    public async Task<IActionResult> ToggleShowOnPos(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();

        product.ShowOnPos = !product.ShowOnPos;
        await _context.SaveChangesAsync();
        return Ok(product);
    }
}
