using FeedFormulation.Domain.Entities.Catalog;
using FeedFormulation.Domain.Enums;
using FeedFormulation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

/// <summary>
/// 
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class IngredientsController : ControllerBase
{

    private readonly AppDbContext _context;

    // We use the same ID of the "Test Farm" that we created in the Seed
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public IngredientsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. LER (GET): Retorna a lista de todos os ingredientes
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var ingredients = await _context.Ingredients
        .Where(i => i.TenantId == _tenantId)
        .OrderBy(i => i.Name)
        .Select(i => new
        {
            i.Id,
            i.Code,
            i.Name,
            i.PriceCurrent,
            Category = i.Category.ToString(),

            Nutrients = i.NutritionalInfo.Select(ni => new
            {
                ni.NutrientId,
                ni.Source,
                ni.Value
            })
        })
        .ToListAsync();

        return Ok(ingredients);
    }

    // 2. CRIAR (POST): Adiciona um novo ingrediente
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateIngredientDto dto)
    {
        var ingredient = new Ingredient(_tenantId, dto.Code, dto.Name, dto.Category)
        {
            PriceCurrent = dto.PriceCurrent
        };

        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Ingrediente criado com sucesso!", id = ingredient.Id });
    }

    // 3. ATUALIZAR (PUT): Altera o preço de um ingrediente existente
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePrice(Guid id, [FromBody] UpdateIngredientDto dto)
    {
        var ingredient = await _context.Ingredients
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == _tenantId);

        if (ingredient == null) return NotFound("Ingrediente não encontrado.");

        ingredient.PriceCurrent = dto.PriceCurrent;

        await _context.SaveChangesAsync();
        return Ok(new { message = "Preço atualizado com sucesso!" });
    }

    // 4. APAGAR (DELETE): Remove o ingrediente do banco
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ingredient = await _context.Ingredients
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == _tenantId);

        if (ingredient == null) return NotFound("Ingrediente não encontrado.");

        _context.Ingredients.Remove(ingredient);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Ingrediente apagado com sucesso!" });
    }
}

// --- DTOs (Formatos de Entrada) ---
// Colocamos aqui no final do arquivo para facilitar por enquanto
public record CreateIngredientDto(string Code, string Name, decimal PriceCurrent, IngredientCategory Category);
public record UpdateIngredientDto(decimal PriceCurrent);