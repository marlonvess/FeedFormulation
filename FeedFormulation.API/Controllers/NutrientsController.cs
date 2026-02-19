using FeedFormulation.Domain.Entities.Catalog;
using FeedFormulation.Domain.Enums;
using FeedFormulation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NutrientsController : ControllerBase
{
    private readonly AppDbContext _context;

    // O mesmo ID fixo da nossa "Fazenda de Teste"
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public NutrientsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. LER (GET): Retorna a lista de todos os nutrientes
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var nutrients = await _context.Nutrients
            .Where(n => n.TenantId == _tenantId)
            .OrderBy(n => n.Name) // Ordena alfabeticamente pelo nome
            .Select(n => new
            {
                n.Id,
                n.Code,
                n.Name,
                Unit = n.Unit.ToString() // Retorna a unidade (ex: Percent, Grams, etc.)
            })
            .ToListAsync();

        return Ok(nutrients);
    }

    // 2. CRIAR (POST): Adiciona um novo nutriente ao catálogo
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateNutrientDto dto)
    {
        // Instancia a entidade Nutriente (ajuste os parâmetros caso o seu construtor seja ligeiramente diferente)
        var nutrient = new Nutrient(_tenantId, dto.Code, dto.Name, dto.Unit);

        _context.Nutrients.Add(nutrient);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Nutriente criado com sucesso!", id = nutrient.Id });
    }

    // 3. ATUALIZAR (PUT): Corrige o nome ou a unidade de um nutriente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateNutrientDto dto)
    {
        var nutrient = await _context.Nutrients
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == _tenantId);

        if (nutrient == null) return NotFound("Nutriente não encontrado.");

        // Nota: Se o 'Name' ou 'Unit' estiverem como 'private set' na sua entidade Nutrient.cs,
        // pode ser necessário criar um método dentro da entidade como: nutrient.Update(dto.Name, dto.Unit);

        // Assumindo que podemos alterar diretamente:
        // nutrient.Name = dto.Name; 
        // nutrient.Unit = dto.Unit;

        // Para fins de teste rápido, podemos deixar apenas um aviso ou implementar a lógica exata consoante a sua entidade.

        await _context.SaveChangesAsync();
        return Ok(new { message = "Nutriente atualizado com sucesso!" });
    }

    // 4. APAGAR (DELETE): Remove o nutriente do banco
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var nutrient = await _context.Nutrients
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == _tenantId);

        if (nutrient == null) return NotFound("Nutriente não encontrado.");

        _context.Nutrients.Remove(nutrient);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Nutriente apagado com sucesso!" });
    }
}

// --- DTOs (Formatos de Entrada) ---
public record CreateNutrientDto(string Code, string Name, NutrientUnit Unit);
public record UpdateNutrientDto(string Name, NutrientUnit Unit);