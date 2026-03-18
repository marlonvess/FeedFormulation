using FeedFormulation.Domain.Entities.Formulation;
using FeedFormulation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConstraintSetsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public ConstraintSetsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. GET: Return a list of all constraint sets for the tenant, including their associated rules
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var constraintSets = await _context.ConstraintSets
            .Where(cs => cs.TenantId == _tenantId)
            .Include(cs => cs.Rules)
            .Select(cs => new
            {
                cs.Id,
                cs.Name,
                cs.AppliesToSpecies,
                cs.AppliesToPhase,
                Rules = cs.Rules.Select(r => new
                {
                    r.Id,
                    r.NutrientId,
                    RuleType = r.Type.ToString(),
                    r.MinValue,
                    r.MaxValue
                })
            })
            .ToListAsync();

        return Ok(constraintSets);
    }

    // 2. POST: Create a new constraint set for the tenant, ensuring that the name is unique within the tenant's constraint sets
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConstraintSetDto dto)
    {
        var constraintSet = new ConstraintSet(_tenantId, dto.Name, dto.Species, dto.Phase);

        _context.ConstraintSets.Add(constraintSet);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Pacote de Restrições criado com sucesso!", id = constraintSet.Id });
    }

    // 3. PUT: Update an existing constraint set's details (name, species, phase), ensuring that the name remains unique within the tenant's constraint sets
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateConstraintSetDto dto)
    {
        var constraintSet = await _context.ConstraintSets
            .FirstOrDefaultAsync(cs => cs.Id == id && cs.TenantId == _tenantId);

        if (constraintSet == null) return NotFound("Pacote não encontrado.");

        // Usamos o método da entidade para alterar os valores com segurança
        constraintSet.UpdateDetails(dto.Name, dto.Species, dto.Phase);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Pacote de Restrições atualizado com sucesso!" });
    }

    // 4. DELETE: Remove a constraint set, ensuring that all associated rules are also deleted (cascade delete)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var constraintSet = await _context.ConstraintSets
            .Include(cs => cs.Rules)
            .FirstOrDefaultAsync(cs => cs.Id == id && cs.TenantId == _tenantId);

        if (constraintSet == null) return NotFound("Pacote não encontrado.");

        _context.ConstraintSets.Remove(constraintSet);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Pacote apagado com sucesso!" });
    }
}

// --- DTOs ---
public record CreateConstraintSetDto(string Name, string? Species, string? Phase);
public record UpdateConstraintSetDto(string Name, string? Species, string? Phase);