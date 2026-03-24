using FeedFormulation.Api.Services;
using FeedFormulation.Domain.Entities.Livestock;
using FeedFormulation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // 🔓 Trabalhadores podem ver a produção
public class MilkProductionController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MilkProductionController(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    private Guid TenantId => _currentUser.GetTenantId();

    [HttpPost]
    [Authorize(Roles = "FarmWorker, Manager, Admin")] // 🔒 Trabalhadores da ordenha inserem leite!
    public async Task<IActionResult> Create([FromBody] CreateMilkProductionDto dto)
    {
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == dto.AnimalId && a.TenantId == TenantId);
        if (animal == null) return NotFound("Animal not found.");

        var record = new MilkProductionRecord(TenantId, dto.AnimalId, dto.Date, dto.VolumeInLiters, dto.FatPercentage, dto.ProteinPercentage, dto.SomaticCellCount);
        _context.MilkProductionRecords.Add(record);

        animal.UpdateProduction(dto.VolumeInLiters);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Milk production registered!", id = record.Id });
    }

    [HttpGet("animal/{animalId}")]
    public async Task<IActionResult> GetByAnimal(Guid animalId)
    {
        var records = await _context.MilkProductionRecords
            .Where(m => m.TenantId == TenantId && m.AnimalId == animalId && !m.IsDeleted)
            .OrderByDescending(m => m.Date).ToListAsync();
        return Ok(records);
    }

    [HttpGet("lot-summary")]
    public async Task<IActionResult> GetLotSummary()
    {
        var dbResult = await _context.MilkProductionRecords.Include(m => m.Animal)
            .Where(m => m.TenantId == TenantId && m.Date >= DateTime.UtcNow.AddDays(-30) && !m.IsDeleted)
            .GroupBy(m => new { m.Date.Date, Lot = m.Animal.Lot ?? "Unassigned" })
            .Select(g => new { RawDate = g.Key.Date, LotName = g.Key.Lot, AverageVolume = g.Average(m => m.VolumeInLiters), NumberOfCows = g.Count() })
            .OrderBy(r => r.RawDate).ToListAsync();

        var result = dbResult.Select(r => new { date = r.RawDate.ToString("yyyy-MM-dd"), lot = r.LotName, averageVolume = Math.Round(r.AverageVolume, 2), numberOfCows = r.NumberOfCows });
        return Ok(result);
    }

    [HttpGet("animal/{animalId}/monthly-evolution")]
    public async Task<IActionResult> GetAnimalMonthlyEvolution(Guid animalId)
    {
        var dbResult = await _context.MilkProductionRecords.Where(m => m.TenantId == TenantId && m.AnimalId == animalId && !m.IsDeleted)
            .GroupBy(m => new { m.Date.Year, m.Date.Month })
            .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, TotalVolume = g.Sum(m => m.VolumeInLiters), AverageDailyVolume = g.Average(m => m.VolumeInLiters), DaysMilked = g.Count() })
            .OrderBy(r => r.Year).ThenBy(r => r.Month).ToListAsync();

        var result = dbResult.Select(r => new { month = $"{r.Year}-{r.Month:D2}", totalVolume = Math.Round(r.TotalVolume, 2), averageDailyVolume = Math.Round(r.AverageDailyVolume, 2), daysMilked = r.DaysMilked });
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager, Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var record = await _context.MilkProductionRecords.FirstOrDefaultAsync(m => m.Id == id && m.TenantId == TenantId && !m.IsDeleted);
        if (record == null) return NotFound();
        record.MarkAsDeleted();
        await _context.SaveChangesAsync();
        return Ok();
    }
}

public record CreateMilkProductionDto(Guid AnimalId, DateTime Date, decimal VolumeInLiters, decimal? FatPercentage, decimal? ProteinPercentage, int? SomaticCellCount);