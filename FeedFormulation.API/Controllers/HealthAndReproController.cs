using FeedFormulation.Api.Services;
using FeedFormulation.Domain.Entities.Livestock;
using FeedFormulation.Domain.Enums;
using FeedFormulation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // 🔓 GETs abertos a qualquer pessoa logada (Tratadores precisam ver os alertas)
public class HealthAndReproController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public HealthAndReproController(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    private Guid TenantId => _currentUser.GetTenantId();

    [HttpPost("reproduction")]
    [Authorize(Roles = "Veterinarian, Manager, Admin")] // 🔒 Só Veterinários/Gestores inserem dados reprodutivos
    public async Task<IActionResult> CreateReproductionEvent([FromBody] CreateReproductionDto dto)
    {
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == dto.AnimalId && a.TenantId == TenantId);
        if (animal == null) return NotFound("Animal not found.");

        var record = new ReproductionRecord(TenantId, dto.AnimalId, dto.Date, dto.EventType, dto.SireId, dto.IsPregnant, dto.Notes);
        _context.ReproductionRecords.Add(record);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Reproduction event registered!", id = record.Id });
    }

    [HttpGet("animal/{animalId}/reproduction")]
    public async Task<IActionResult> GetAnimalReproductionHistory(Guid animalId)
    {
        var records = await _context.ReproductionRecords.Where(r => r.TenantId == TenantId && r.AnimalId == animalId && !r.IsDeleted)
            .OrderByDescending(r => r.Date)
            .Select(r => new { id = r.Id, date = r.Date.ToString("yyyy-MM-dd"), eventType = r.EventType.ToString(), sireId = r.SireId, isPregnant = r.IsPregnant, notes = r.Notes })
            .ToListAsync();
        return Ok(records);
    }

    [HttpPost("health")]
    [Authorize(Roles = "Veterinarian, Manager, Admin")] // 🔒 Só Veterinários/Gestores inserem doenças
    public async Task<IActionResult> CreateHealthEvent([FromBody] CreateHealthDto dto)
    {
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == dto.AnimalId && a.TenantId == TenantId);
        if (animal == null) return NotFound("Animal not found.");

        var record = new HealthRecord(TenantId, dto.AnimalId, dto.Date, dto.EventType, dto.DiagnosisOrName, dto.Treatment, dto.Cost);
        _context.HealthRecords.Add(record);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Health event registered!", id = record.Id });
    }

    [HttpGet("animal/{animalId}/health")]
    public async Task<IActionResult> GetAnimalHealthHistory(Guid animalId)
    {
        var records = await _context.HealthRecords.Where(h => h.TenantId == TenantId && h.AnimalId == animalId && !h.IsDeleted)
            .OrderByDescending(h => h.Date)
            .Select(h => new { id = h.Id, date = h.Date.ToString("yyyy-MM-dd"), eventType = h.EventType.ToString(), diagnosis = h.DiagnosisOrName, treatment = h.Treatment, cost = h.Cost })
            .ToListAsync();
        return Ok(records);
    }

    [HttpGet("dashboard-alerts")]
    public async Task<IActionResult> GetDashboardAlerts()
    {
        var today = DateTime.UtcNow.Date;
        var recentHealthCosts = await _context.HealthRecords.Where(h => h.TenantId == TenantId && h.Date >= today.AddDays(-30) && !h.IsDeleted).SumAsync(h => h.Cost ?? 0);

        var femaleCows = await _context.Animals.Where(a => a.TenantId == TenantId && a.Status == AnimalStatus.Active && a.Gender == AnimalGender.Female).ToListAsync();
        var reproRecords = await _context.ReproductionRecords.Where(r => r.TenantId == TenantId && !r.IsDeleted).ToListAsync();

        var negativePregnancyChecks = new List<object>();
        var cowsToDryOff = new List<object>();
        var overdueForInsemination = new List<object>();

        foreach (var animal in femaleCows)
        {
            var lastReproEvent = reproRecords.Where(r => r.AnimalId == animal.Id).OrderByDescending(r => r.Date).FirstOrDefault();
            if (lastReproEvent != null)
            {
                var daysSinceLastEvent = (today - lastReproEvent.Date).TotalDays;
                if (lastReproEvent.EventType == ReproductionEventType.PregnancyCheck && lastReproEvent.IsPregnant == false)
                    negativePregnancyChecks.Add(new { animalName = animal.Name, sia = animal.SiaNumber, reason = "Diagnóstico negativo" });
                if ((lastReproEvent.EventType == ReproductionEventType.Insemination && daysSinceLastEvent >= 210 && daysSinceLastEvent <= 240) ||
                    (lastReproEvent.EventType == ReproductionEventType.PregnancyCheck && lastReproEvent.IsPregnant == true && daysSinceLastEvent >= 180))
                    cowsToDryOff.Add(new { animalName = animal.Name, sia = animal.SiaNumber, reason = "Período de secagem" });
                if (lastReproEvent.EventType == ReproductionEventType.Calving && daysSinceLastEvent > 60)
                    overdueForInsemination.Add(new { animalName = animal.Name, sia = animal.SiaNumber, daysSinceCalving = Math.Floor(daysSinceLastEvent) });
            }
        }

        return Ok(new { TotalHealthCostsLast30Days = Math.Round(recentHealthCosts, 2), Alerts = new { NegativePregnancyChecks = negativePregnancyChecks, CowsToDryOff = cowsToDryOff, OverdueForInsemination = overdueForInsemination } });
    }

    [HttpDelete("reproduction/{id}")]
    [Authorize(Roles = "Manager, Admin")]
    public async Task<IActionResult> DeleteReproduction(Guid id)
    {
        var record = await _context.ReproductionRecords.FirstOrDefaultAsync(r => r.Id == id && r.TenantId == TenantId && !r.IsDeleted);
        if (record == null) return NotFound();
        record.MarkAsDeleted();
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("health/{id}")]
    [Authorize(Roles = "Manager, Admin")]
    public async Task<IActionResult> DeleteHealth(Guid id)
    {
        var record = await _context.HealthRecords.FirstOrDefaultAsync(h => h.Id == id && h.TenantId == TenantId && !h.IsDeleted);
        if (record == null) return NotFound();
        record.MarkAsDeleted();
        await _context.SaveChangesAsync();
        return Ok();
    }
}

public record CreateReproductionDto(Guid AnimalId, DateTime Date, ReproductionEventType EventType, string? SireId, bool? IsPregnant, string? Notes);
public record CreateHealthDto(Guid AnimalId, DateTime Date, HealthEventType EventType, string DiagnosisOrName, string? Treatment, decimal? Cost);