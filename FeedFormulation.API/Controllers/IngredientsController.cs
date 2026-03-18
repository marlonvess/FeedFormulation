using FeedFormulation.Domain.Entities.Catalog;
using FeedFormulation.Domain.Enums;
using FeedFormulation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

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

    // 1.GET: REturn a list of all ingredients for the tenant, ordered by name, including their current price and category
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
            Category = i.Category.ToString()       
        })
        .ToListAsync();

        return Ok(ingredients);
    }

    // 2.POST: Add a new ingredient to the database, ensuring that the code is unique within the tenant. The request body should include the code, name, current price, and category of the ingredient.
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

    // 3.PUT: Change the current price of an existing ingredient, ensuring that the ingredient belongs to the tenant. The request body should include the new price.
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

    // 4. DELETE: Remove an ingredient from the database, ensuring that it belongs to the tenant. The ingredient should only be deleted if it is not currently used in any feed formulation. If it is used, return an appropriate error message.
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

// --- DTOs ---

public record CreateIngredientDto(string Code, string Name, decimal PriceCurrent, IngredientCategory Category);
public record UpdateIngredientDto(decimal PriceCurrent);