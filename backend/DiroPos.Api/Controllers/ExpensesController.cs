using DiroPos.Api.Data;
using DiroPos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiroPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ExpensesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Expense>>> GetExpenses(
        [FromQuery] DateTime? date = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int? month = null,
        [FromQuery] int? year = null,
        [FromQuery] string? category = null,
        [FromQuery] int limit = 200)
    {
        var query = _context.Expenses.AsQueryable();

        if (fromDate.HasValue && toDate.HasValue)
        {
            var start = fromDate.Value.Date;
            var end = toDate.Value.Date.AddDays(1);
            query = query.Where(e => e.Date >= start && e.Date < end);
        }
        else if (fromDate.HasValue)
        {
            var start = fromDate.Value.Date;
            query = query.Where(e => e.Date >= start);
        }
        else if (toDate.HasValue)
        {
            var end = toDate.Value.Date.AddDays(1);
            query = query.Where(e => e.Date < end);
        }
        else if (date.HasValue)
        {
            var targetDate = date.Value.Date;
            var nextDate = targetDate.AddDays(1);
            query = query.Where(e => e.Date >= targetDate && e.Date < nextDate);
        }
        else if (month.HasValue && year.HasValue)
        {
            var start = new DateTime(year.Value, month.Value, 1);
            var end = start.AddMonths(1);
            query = query.Where(e => e.Date >= start && e.Date < end);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(e => e.Category.ToLower() == category.ToLower());
        }

        return await query.OrderByDescending(e => e.Date).Take(limit).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Expense>> GetExpense(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null) return NotFound();
        return expense;
    }

    [HttpPost]
    public async Task<ActionResult<Expense>> CreateExpense(Expense expense)
    {
        if (expense.Amount <= 0)
        {
            return BadRequest("Số tiền chi phải lớn hơn 0.");
        }

        if (expense.Date == default)
        {
            expense.Date = DateTime.UtcNow.AddHours(7);
        }

        expense.CreatedAt = DateTime.UtcNow.AddHours(7);
        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, expense);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(int id, Expense expense)
    {
        if (id != expense.Id) return BadRequest();

        _context.Entry(expense).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Expenses.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null) return NotFound();

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("categories")]
    public ActionResult<IEnumerable<string>> GetCategories()
    {
        var categories = new[]
        {
            "Mặt bằng & Tiện ích",      // Tiền thuê nhà, điện, nước, internet, rác
            "Phụ liệu & Hóa chất",     // Lưỡi dao cạo, bọt cạo, thuốc uốn nhuộm, khăn giấy
            "Dụng cụ & Máy móc",       // Kéo cắt tóc, tông đơ, máy sấy, dầu tra tông đơ, lược
            "Sinh hoạt & Ăn uống",     // Cơm trưa, cà phê, nước ngọt trong ca
            "Khác"                     // Chi phí phát sinh khác
        };
        return Ok(categories);
    }
}
