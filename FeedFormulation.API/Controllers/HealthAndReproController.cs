using FeedFormulation.Domain.Entities.Livestock;
using FeedFormulation.Domain.Enums;
using FeedFormulation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthAndReproController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public HealthAndReproController(AppDbContext context)
    {
        _context = context;
    }

    // ==========================================
    // --- MÓDULO DE REPRODUÇÃO ---
    // ==========================================

    // 1. POST: Regista um novo evento reprodutivo
    [HttpPost("reproduction")]
    public async Task<IActionResult> CreateReproductionEvent([FromBody] CreateReproductionDto dto)
    {
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == dto.AnimalId && a.TenantId == _tenantId);
        if (animal == null) return NotFound("Animal not found.");

        var record = new ReproductionRecord(
            _tenantId, dto.AnimalId, dto.Date, dto.EventType, dto.SireId, dto.IsPregnant, dto.Notes
        );

        _context.ReproductionRecords.Add(record);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Reproduction event successfully registered!", id = record.Id });
    }

    // 2. GET: Histórico reprodutivo de uma vaca
    [HttpGet("animal/{animalId}/reproduction")]
    public async Task<IActionResult> GetAnimalReproductionHistory(Guid animalId)
    {
        var records = await _context.ReproductionRecords
            .Where(r => r.TenantId == _tenantId && r.AnimalId == animalId && !r.IsDeleted)
            .OrderByDescending(r => r.Date)
            .Select(r => new
            {
                id = r.Id,
                date = r.Date.ToString("yyyy-MM-dd"),
                eventType = r.EventType.ToString(),
                sireId = r.SireId,
                isPregnant = r.IsPregnant,
                notes = r.Notes
            })
            .ToListAsync();

        return Ok(records);
    }

    // ==========================================
    // --- MÓDULO DE SAÚDE ---
    // ==========================================

    // 3. POST: Regista um novo evento de saúde
    [HttpPost("health")]
    public async Task<IActionResult> CreateHealthEvent([FromBody] CreateHealthDto dto)
    {
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == dto.AnimalId && a.TenantId == _tenantId);
        if (animal == null) return NotFound("Animal not found.");

        var record = new HealthRecord(
            _tenantId, dto.AnimalId, dto.Date, dto.EventType, dto.DiagnosisOrName, dto.Treatment, dto.Cost
        );

        _context.HealthRecords.Add(record);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Health event successfully registered!", id = record.Id });
    }

    // 4. GET: Histórico de saúde de uma vaca
    [HttpGet("animal/{animalId}/health")]
    public async Task<IActionResult> GetAnimalHealthHistory(Guid animalId)
    {
        var records = await _context.HealthRecords
            .Where(h => h.TenantId == _tenantId && h.AnimalId == animalId && !h.IsDeleted)
            .OrderByDescending(h => h.Date)
            .Select(h => new
            {
                id = h.Id,
                date = h.Date.ToString("yyyy-MM-dd"),
                eventType = h.EventType.ToString(),
                diagnosis = h.DiagnosisOrName,
                treatment = h.Treatment,
                cost = h.Cost
            })
            .ToListAsync();

        return Ok(records);
    }

    // ==========================================
    // --- BUSINESS INTELLIGENCE & ALERTAS ---
    // ==========================================

    // 5. GET: Alertas de Gestão Avançados (Versão à prova de falhas)
    [HttpGet("dashboard-alerts")]
    public async Task<IActionResult> GetDashboardAlerts()
    {
        var today = DateTime.UtcNow.Date;
        var thirtyDaysAgo = today.AddDays(-30);

        // 1. Calcula Despesas Médicas
        var recentHealthCosts = await _context.HealthRecords
            .Where(h => h.TenantId == _tenantId && h.Date >= thirtyDaysAgo && !h.IsDeleted)
            .SumAsync(h => h.Cost ?? 0);

        // 2. Carrega as Vacas e os Registos de forma explícita (Resolve o bug do EF Core)
        var femaleCows = await _context.Animals
            .Where(a => a.TenantId == _tenantId && a.Status == AnimalStatus.Active && a.Gender == AnimalGender.Female)
            .ToListAsync();

        var reproRecords = await _context.ReproductionRecords
            .Where(r => r.TenantId == _tenantId && !r.IsDeleted)
            .ToListAsync();

        var negativePregnancyChecks = new List<object>();
        var cowsToDryOff = new List<object>();
        var overdueForInsemination = new List<object>();

        foreach (var animal in femaleCows)
        {
            // Filtra o último evento só para esta vaca específica
            var lastReproEvent = reproRecords
                .Where(r => r.AnimalId == animal.Id)
                .OrderByDescending(r => r.Date)
                .FirstOrDefault();

            if (lastReproEvent != null)
            {
                var daysSinceLastEvent = (today - lastReproEvent.Date).TotalDays;

                // Alerta A: Diagnóstico Negativo (Vazia)
                if (lastReproEvent.EventType == ReproductionEventType.PregnancyCheck && lastReproEvent.IsPregnant == false)
                {
                    negativePregnancyChecks.Add(new
                    {
                        animalName = animal.Name,
                        sia = animal.SiaNumber,
                        reason = "Diagnóstico de gestação negativo. Requer atenção."
                    });
                }

                // Alerta B: Vacas para Secar (~220 dias após Inseminação)
                if ((lastReproEvent.EventType == ReproductionEventType.Insemination && daysSinceLastEvent >= 210 && daysSinceLastEvent <= 240) ||
                    (lastReproEvent.EventType == ReproductionEventType.PregnancyCheck && lastReproEvent.IsPregnant == true && daysSinceLastEvent >= 180))
                {
                    cowsToDryOff.Add(new
                    {
                        animalName = animal.Name,
                        sia = animal.SiaNumber,
                        reason = "Atingiu o período de secagem (~60 dias pré-parto)."
                    });
                }

                // Alerta C: Atrasadas para Inseminação (> 60 dias após parto)
                if (lastReproEvent.EventType == ReproductionEventType.Calving && daysSinceLastEvent > 60)
                {
                    overdueForInsemination.Add(new
                    {
                        animalName = animal.Name,
                        sia = animal.SiaNumber,
                        daysSinceCalving = Math.Floor(daysSinceLastEvent),
                        reason = $"Pariu há {Math.Floor(daysSinceLastEvent)} dias e não foi inseminada."
                    });
                }
            }
        }

        return Ok(new
        {
            TotalHealthCostsLast30Days = Math.Round(recentHealthCosts, 2),
            Alerts = new
            {
                NegativePregnancyChecks = negativePregnancyChecks,
                CowsToDryOff = cowsToDryOff,
                OverdueForInsemination = overdueForInsemination
            }
        });
    }

    // ==========================================
    // --- SOFT DELETES ---
    // ==========================================

    [HttpDelete("reproduction/{id}")]
    public async Task<IActionResult> DeleteReproduction(Guid id)
    {
        var record = await _context.ReproductionRecords.FirstOrDefaultAsync(r => r.Id == id && r.TenantId == _tenantId && !r.IsDeleted);
        if (record == null) return NotFound();

        record.MarkAsDeleted();
        await _context.SaveChangesAsync();

        return Ok(new { message = "Reproduction record deleted (Soft Delete)." });
    }

    [HttpDelete("health/{id}")]
    public async Task<IActionResult> DeleteHealth(Guid id)
    {
        var record = await _context.HealthRecords.FirstOrDefaultAsync(h => h.Id == id && h.TenantId == _tenantId && !h.IsDeleted);
        if (record == null) return NotFound();

        record.MarkAsDeleted();
        await _context.SaveChangesAsync();

        return Ok(new { message = "Health record deleted (Soft Delete)." });
    }
}

// ==========================================
// --- DTOs (Data Transfer Objects) ---
// ==========================================

public record CreateReproductionDto(
    Guid AnimalId,
    DateTime Date,
    ReproductionEventType EventType,
    string? SireId,
    bool? IsPregnant,
    string? Notes
);

public record CreateHealthDto(
    Guid AnimalId,
    DateTime Date,
    HealthEventType EventType,
    string DiagnosisOrName,
    string? Treatment,
    decimal? Cost
);