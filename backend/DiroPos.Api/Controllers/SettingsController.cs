using DiroPos.Api.Data;
using DiroPos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiroPos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SettingsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<ShopSetting>> GetSetting()
    {
        var setting = await _context.ShopSettings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new ShopSetting();
            _context.ShopSettings.Add(setting);
            await _context.SaveChangesAsync();
        }
        return Ok(setting);
    }

    [HttpPut]
    public async Task<ActionResult<ShopSetting>> UpdateSetting([FromBody] ShopSetting input)
    {
        var setting = await _context.ShopSettings.FirstOrDefaultAsync();
        if (setting == null)
        {
            setting = new ShopSetting();
            _context.ShopSettings.Add(setting);
        }

        setting.ShopName = input.ShopName;
        setting.Address = input.Address;
        setting.Phone = input.Phone;
        setting.Slogan = input.Slogan;
        setting.BankId = input.BankId;
        setting.BankName = input.BankName;
        setting.AccountNo = input.AccountNo;
        setting.AccountName = input.AccountName;
        setting.QrTemplate = input.QrTemplate;

        await _context.SaveChangesAsync();
        return Ok(setting);
    }
}
