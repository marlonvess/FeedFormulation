using FeedFormulation.Domain.Entities.Formulation;
using FeedFormulation.Domain.Dtos.Solver;
using FeedFormulation.Domain.Enums;
using FeedFormulation.Domain.ValueObjects;
using FeedFormulation.Infrastructure.Persistence;
using FeedFormulation.Infrastructure.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FormulasController : ControllerBase
{
    /// <summary>
    ///
    /// </summary>
    private readonly AppDbContext _context;
    private readonly SolverHttpClient _solverClient;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public FormulasController(AppDbContext context, SolverHttpClient solverClient)
    {
        _context = context;
        _solverClient = solverClient;
    }

    // GET /api/Formulas
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var formulas = await _context.Formulas
            .Where(f => f.TenantId == _tenantId)
            .Select(f => new { f.Id, f.Name, f.Description, Status = f.Status.ToString() })
            .ToListAsync();

        return Ok(formulas);
    }

    // GET /api/Formulas/{id}  — retorna a fórmula com todas as versões e linhas
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var formula = await _context.Formulas
            .Include(f => f.Versions).ThenInclude(v => v.Lines)
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == _tenantId);

        if (formula == null) return NotFound("Fórmula não encontrada.");

        return Ok(formula);
    }

    // POST /api/Formulas
    // Body: { "name": "Ração Leitão L1", "description": "..." }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFormulaDto dto)
    {
        var formula = new Formula(_tenantId, dto.Name, dto.Description);
        _context.Formulas.Add(formula);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Fórmula criada!", id = formula.Id });
    }

    // DELETE /api/Formulas/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var formula = await _context.Formulas.FirstOrDefaultAsync(f => f.Id == id && f.TenantId == _tenantId);
        if (formula == null) return NotFound("Fórmula não encontrada.");

        _context.Formulas.Remove(formula);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Fórmula apagada." });
    }


    // ── VERSÕES ──────────────────────────────────────────────

    // POST /api/Formulas/{formulaId}/versions
    // Body: { "targetBatchSizeKg": 1000, "constraintSetId": null, "notes": "...",
    //         "lines": [{ "ingredientId": "...", "minPercent": 10, "maxPercent": 60, "fixedPercent": null }] }
    [HttpPost("{formulaId}/versions")]
    public async Task<IActionResult> AddVersion(Guid formulaId, [FromBody] CreateVersionDto dto)
    {
        var formula = await _context.Formulas
            .Include(f => f.Versions)
            .FirstOrDefaultAsync(f => f.Id == formulaId && f.TenantId == _tenantId);

        if (formula == null) return NotFound("Fórmula não encontrada.");

        var nextVersion = formula.Versions.Any() ? formula.Versions.Max(v => v.VersionNumber) + 1 : 1;

        var version = new FormulaVersion(_tenantId, formulaId, nextVersion, dto.TargetBatchSizeKg, dto.ConstraintSetId, dto.Notes);
        _context.FormulaVersions.Add(version);
        await _context.SaveChangesAsync();

        if (dto.Lines != null && dto.Lines.Any())
        {
            var lines = dto.Lines.Select(l => new FormulaLine(
                _tenantId, version.Id, l.IngredientId,
                l.MinPercent.HasValue ? (Percentage)l.MinPercent.Value : null,
                l.MaxPercent.HasValue ? (Percentage)l.MaxPercent.Value : null,
                l.FixedPercent.HasValue ? (Percentage)l.FixedPercent.Value : null
            ));
            _context.FormulaLines.AddRange(lines);
            await _context.SaveChangesAsync();
        }

        return Ok(new { message = $"Versão {nextVersion} criada!", versionId = version.Id });
    }

    // DELETE /api/Formulas/{formulaId}/versions/{versionId}
    [HttpDelete("{formulaId}/versions/{versionId}")]
    public async Task<IActionResult> DeleteVersion(Guid formulaId, Guid versionId)
    {
        var version = await _context.FormulaVersions
            .FirstOrDefaultAsync(v => v.Id == versionId && v.FormulaId == formulaId && v.TenantId == _tenantId);

        if (version == null) return NotFound("Versão não encontrada.");
        if (version.IsApproved) return BadRequest("Não é possível apagar uma versão aprovada.");

        _context.FormulaVersions.Remove(version);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Versão apagada." });
    }

    // PUT /api/Formulas/{formulaId}/versions/{versionId}/approve
    [HttpPut("{formulaId}/versions/{versionId}/approve")]
    public async Task<IActionResult> ApproveVersion(Guid formulaId, Guid versionId)
    {
        var version = await _context.FormulaVersions
            .FirstOrDefaultAsync(v => v.Id == versionId && v.FormulaId == formulaId && v.TenantId == _tenantId);

        if (version == null) return NotFound("Versão não encontrada.");
        if (version.IsApproved) return BadRequest("Versão já aprovada.");

        version.Approve(Guid.Empty);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Versão aprovada.", approvedAt = version.ApprovedAt });
    }


    // ── SOLVER ───────────────────────────────────────────────

    // POST /api/Formulas/{formulaId}/versions/{versionId}/solve
    [HttpPost("{formulaId}/versions/{versionId}/solve")]
    public async Task<IActionResult> Solve(Guid formulaId, Guid versionId)
    {
        var version = await _context.FormulaVersions
            .Include(v => v.Lines)
            .FirstOrDefaultAsync(v => v.Id == versionId && v.FormulaId == formulaId && v.TenantId == _tenantId);

        if (version == null) return NotFound("Versão não encontrada.");
        if (!version.Lines.Any()) return BadRequest("A versão não tem ingredientes.");

        var ingredientIds = version.Lines.Select(l => l.IngredientId).ToList();
        var ingredients = await _context.Ingredients
            .Include(i => i.NutritionalInfo)
            .Where(i => ingredientIds.Contains(i.Id))
            .ToListAsync();

        // Ingredientes para o solver
        var solverIngredients = version.Lines.Select(line =>
        {
            var ing = ingredients.First(i => i.Id == line.IngredientId);
            return new SolverIngredientInput(
                IngredientId: ing.Id.ToString(),
                Code: ing.Code,
                CostPerKg: ing.PriceCurrent,
                MinInclusionPercent: line.InclusionMinPercent?.Value,
                MaxInclusionPercent: line.InclusionMaxPercent?.Value,
                FixedInclusionPercent: line.FixedInclusionPercent?.Value
            );
        }).ToList();

        // Perfis nutricionais
        var nutrientProfiles = ingredients
            .SelectMany(ing => ing.NutritionalInfo.Select(ni => new SolverNutrientProfileInput(
                IngredientId: ing.Id.ToString(),
                NutrientId: ni.NutrientId.ToString(),
                Value: ni.Value
            ))).ToList();

        // Restrições do ConstraintSet (se houver)
        var constraints = new List<SolverConstraintInput>();
        if (version.ConstraintSetId.HasValue)
        {
            var rules = await _context.ConstraintRules
                .Where(r => r.ConstraintSetId == version.ConstraintSetId.Value)
                .ToListAsync();

            constraints = rules.Select(r => new SolverConstraintInput(
                Type: r.Type.ToString().ToLower(),
                NutrientId: r.NutrientId?.ToString(),
                MinValue: r.MinValue,
                MaxValue: r.MaxValue
            )).ToList();
        }

        var request = new SolverProblemRequest(
            TenantId: _tenantId.ToString(),
            FormulaVersionId: versionId.ToString(),
            TargetBatchSizeKg: version.TargetBatchSizeKg,
            Objective: "min_cost",
            Ingredients: solverIngredients,
            Constraints: constraints,
            NutrientProfiles: nutrientProfiles
        );

        // Regista o SolverRun
        var requestJson = System.Text.Json.JsonSerializer.Serialize(request);
        var solverRun = new FeedFormulation.Domain.Entities.Solver.SolverRun(
            _tenantId, versionId, Guid.Empty, requestJson);
        solverRun.MarkRunning();
        _context.SolverRuns.Add(solverRun);
        await _context.SaveChangesAsync();

        // Chama o Python
        SolverProblemResponse response;
        try
        {
            response = await _solverClient.SolveAsync(request);
        }
        catch (Exception ex)
        {
            solverRun.MarkFailed(ex.Message);
            await _context.SaveChangesAsync();
            return StatusCode(502, new { message = "Solver inacessível.", error = ex.Message });
        }

        // Guarda o resultado
        var responseJson = System.Text.Json.JsonSerializer.Serialize(response);
        if (response.Status == "succeeded") solverRun.MarkSucceeded(responseJson);
        else if (response.Status == "infeasible") solverRun.MarkInfeasible(response.DiagnosticMessage ?? "Inviável.");
        else solverRun.MarkFailed(response.DiagnosticMessage ?? "Erro.");
        await _context.SaveChangesAsync();

        return Ok(new
        {
            status = response.Status,
            totalCost = response.TotalCost,
            ingredientResults = response.IngredientResults,
            nutrientResults = response.NutrientResults,
            diagnosticMessage = response.DiagnosticMessage,
            solverRunId = solverRun.Id
        });
    }
}

// DTOs
public record CreateFormulaDto(string Name, string? Description);
public record CreateVersionDto(decimal TargetBatchSizeKg, Guid? ConstraintSetId, string? Notes, List<CreateLineDto>? Lines);
public record CreateLineDto(Guid IngredientId, decimal? MinPercent, decimal? MaxPercent, decimal? FixedPercent);