using FeedFormulation.Domain.Entities.Livestock;
using FeedFormulation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MilkProductionController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public MilkProductionController(AppDbContext context)
    {
        _context = context;
    }

    // 1. POST: Regista uma nova ordenha/pesagem
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMilkProductionDto dto)
    {
        // Verifica se o animal existe no nosso rebanho
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == dto.AnimalId && a.TenantId == _tenantId);
        if (animal == null) return NotFound("Animal not found.");

        var record = new MilkProductionRecord(
            _tenantId,
            dto.AnimalId,
            dto.Date,
            dto.VolumeInLiters,
            dto.FatPercentage,
            dto.ProteinPercentage,
            dto.SomaticCellCount
        );

        _context.MilkProductionRecords.Add(record);

        // MÁGICA: Atualiza automaticamente a ficha principal da vaca com esta última produção!
        animal.UpdateProduction(dto.VolumeInLiters);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Milk production successfully registered!", id = record.Id });
    }

    // 2. GET: Histórico de produção de uma vaca específica (Para o Perfil da Vaca)
    [HttpGet("animal/{animalId}")]
    public async Task<IActionResult> GetByAnimal(Guid animalId)
    {
        var records = await _context.MilkProductionRecords
            .Where(m => m.TenantId == _tenantId && m.AnimalId == animalId)
            .OrderByDescending(m => m.Date) // Do mais recente para o mais antigo
            .Select(m => new
            {
                id = m.Id,
                date = m.Date.ToString("yyyy-MM-dd"),
                volumeInLiters = m.VolumeInLiters,
                fatPercentage = m.FatPercentage,
                proteinPercentage = m.ProteinPercentage,
                somaticCellCount = m.SomaticCellCount
            })
            .ToListAsync();

        return Ok(records);
    }

    // 3. GET: Resumo diário de todo o rebanho (Para o Gráfico do Dashboard no React)
    [HttpGet("summary")]
    public async Task<IActionResult> GetHerdSummary()
    {
        // Pega os dados dos últimos 30 dias
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        // PASSO 1: Deixa o PostgreSQL fazer a matemática pesada (Sem formatar a data)
        var dbResult = await _context.MilkProductionRecords
            .Where(m => m.TenantId == _tenantId && m.Date >= thirtyDaysAgo)
            .GroupBy(m => m.Date.Date)
            .Select(g => new
            {
                RawDate = g.Key, // Data crua, o SQL entende isto!
                totalVolume = g.Sum(m => m.VolumeInLiters),
                averagePerCow = g.Average(m => m.VolumeInLiters),
                numberOfCowsMilked = g.Select(m => m.AnimalId).Distinct().Count()
            })
            .OrderBy(r => r.RawDate)
            .ToListAsync(); // <-- AQUI os dados saem do banco e vão para a memória do C#

        // PASSO 2: O C# pega nos dados em memória e transforma a data em texto
        var records = dbResult.Select(r => new
        {
            date = r.RawDate.ToString("yyyy-MM-dd"), // O C# formata sem problemas!
            totalVolume = r.totalVolume,
            averagePerCow = r.averagePerCow,
            numberOfCowsMilked = r.numberOfCowsMilked
        });

        return Ok(records);
    }

    // 4. PUT: Atualiza um registo de ordenha existente (Correção de erros)
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMilkProductionDto dto)
    {
        var record = await _context.MilkProductionRecords
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == _tenantId);

        if (record == null) return NotFound("Milk production record not found.");

        // Atualiza os valores usando os métodos seguros do Domínio
        record.SetVolume(dto.VolumeInLiters);
        record.UpdateQuality(dto.FatPercentage, dto.ProteinPercentage, dto.SomaticCellCount);

        // MÁGICA EXTRA: Se alterarmos o volume, vamos também atualizar a ficha do Animal
        // para garantir que o "LastMilkProduction" não fica com o valor antigo errado.
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == record.AnimalId);
        if (animal != null)
        {
            animal.UpdateProduction(dto.VolumeInLiters);
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Milk production successfully updated!" });
    }

    // 5. DELETE: Soft Delete (Oculta o registo, mas mantém no banco)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var record = await _context.MilkProductionRecords
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == _tenantId && !m.IsDeleted);

        if (record == null) return NotFound("Milk production record not found or already deleted.");

        // Em vez de _context.Remove(record);
        record.MarkAsDeleted();

        await _context.SaveChangesAsync();

        return Ok(new { message = "Milk production record successfully deleted!" });
    }

    // --- BUSINESS INTELLIGENCE (BI) ENDPOINTS ---

    // 6. GET: Média Diária por Lote (Para o Gráfico de Lotes)
    [HttpGet("lot-summary")]
    public async Task<IActionResult> GetLotSummary()
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        // PASSO 1: Fazer a matemática pesada no PostgreSQL
        // Repare no .Include(m => m.Animal) -> Isto faz o JOIN para sabermos o lote da vaca!
        var dbResult = await _context.MilkProductionRecords
            .Include(m => m.Animal)
            .Where(m => m.TenantId == _tenantId && m.Date >= thirtyDaysAgo && !m.IsDeleted)
            .GroupBy(m => new {
                m.Date.Date,
                Lot = m.Animal.Lot ?? "Unassigned" // Se não tiver lote, chamamos de "Unassigned" (Sem Lote)
            })
            .Select(g => new
            {
                RawDate = g.Key.Date,
                LotName = g.Key.Lot,
                AverageVolume = g.Average(m => m.VolumeInLiters),
                NumberOfCows = g.Count()
            })
            .OrderBy(r => r.RawDate)
            .ThenBy(r => r.LotName)
            .ToListAsync();

        // PASSO 2: Formatar a data no C# e arredondar as casas decimais para o React
        var result = dbResult.Select(r => new
        {
            date = r.RawDate.ToString("yyyy-MM-dd"),
            lot = r.LotName,
            averageVolume = Math.Round(r.AverageVolume, 2),
            numberOfCows = r.NumberOfCows
        });

        return Ok(result);
    }

    // 7. GET: Evolução Mensal da Vaca (Para o Perfil Individual do Animal)
    [HttpGet("animal/{animalId}/monthly-evolution")]
    public async Task<IActionResult> GetAnimalMonthlyEvolution(Guid animalId)
    {
        // PASSO 1: O PostgreSQL agrupa por Ano e Mês
        var dbResult = await _context.MilkProductionRecords
            .Where(m => m.TenantId == _tenantId && m.AnimalId == animalId && !m.IsDeleted)
            .GroupBy(m => new { m.Date.Year, m.Date.Month })
            .Select(g => new
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                TotalVolume = g.Sum(m => m.VolumeInLiters),
                AverageDailyVolume = g.Average(m => m.VolumeInLiters),
                DaysMilked = g.Count()
            })
            .OrderBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ToListAsync();

        // PASSO 2: O C# formata o texto para "2026-03" (Ano-Mês)
        var result = dbResult.Select(r => new
        {
            month = $"{r.Year}-{r.Month:D2}", // :D2 garante que o mês 3 fica "03"
            totalVolume = Math.Round(r.TotalVolume, 2),
            averageDailyVolume = Math.Round(r.AverageDailyVolume, 2),
            daysMilked = r.DaysMilked
        });

        return Ok(result);
    }
}



// --- DTOs ---
public record CreateMilkProductionDto(
    Guid AnimalId,
    DateTime Date,
    decimal VolumeInLiters,
    decimal? FatPercentage, // Opcional
    decimal? ProteinPercentage, // Opcional
    int? SomaticCellCount // Opcional
);

public record UpdateMilkProductionDto(
    decimal VolumeInLiters,
    decimal? FatPercentage,
    decimal? ProteinPercentage,
    int? SomaticCellCount
);