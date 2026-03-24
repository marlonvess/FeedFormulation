using FeedFormulation.Api.Services;
using FeedFormulation.Domain.Entities.Formulation;
using FeedFormulation.Domain.Dtos.Solver;
using FeedFormulation.Infrastructure.Persistence;
using FeedFormulation.Infrastructure.Http;
using FeedFormulation.Domain.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Nutritionist, Manager, Admin")] // 🔒 BLOQUEIO: Só peritos em nutrição e gestão entram!
public class FormulasController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly SolverHttpClient _solverClient;
    private readonly ICurrentUserService _currentUser;

    public FormulasController(AppDbContext context, SolverHttpClient solverClient, ICurrentUserService currentUser)
    {
        _context = context;
        _solverClient = solverClient;
        _currentUser = currentUser;
    }

    private Guid TenantId => _currentUser.GetTenantId();

    // 1. GET: Lista as fórmulas da fazenda
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var formulas = await _context.Formulas
            .Where(f => f.TenantId == TenantId)
            .Select(f => new { f.Id, f.Name, f.Description, Status = f.Status.ToString() })
            .ToListAsync();

        return Ok(formulas);
    }

    // 2. GET: Detalhes completos de uma fórmula
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var formula = await _context.Formulas
            .Include(f => f.Versions).ThenInclude(v => v.Lines)
            .FirstOrDefaultAsync(f => f.Id == id && f.TenantId == TenantId);

        if (formula == null) return NotFound("Fórmula não encontrada.");
        return Ok(formula);
    }

    // ==========================================
    // 🧠 O NOVO MOTOR "ONE-SHOT" FLEXÍVEL
    // ==========================================

    [HttpPost("create-and-solve")]
    public async Task<IActionResult> CreateAndSolve([FromBody] AdvancedFormulaDto dto)
    {
        // 1. Criar a Estrutura Base
        var formula = new Formula(TenantId, dto.Name, dto.Description);
        _context.Formulas.Add(formula);

        var version = new FormulaVersion(TenantId, formula.Id, 1, dto.TargetBatchSizeKg, dto.ConstraintSetId, dto.Notes);
        _context.FormulaVersions.Add(version);

        // 2. Adicionar os Ingredientes com os seus limites
        var lines = dto.Lines.Select(l => new FormulaLine(
            TenantId, version.Id, l.IngredientId,
            l.MinPercent.HasValue ? (Percentage)l.MinPercent.Value : null,
            l.MaxPercent.HasValue ? (Percentage)l.MaxPercent.Value : null,
            l.FixedPercent.HasValue ? (Percentage)l.FixedPercent.Value : null
        )).ToList();
        _context.FormulaLines.AddRange(lines);

        await _context.SaveChangesAsync(); // Guardar para ter IDs gerados

        // 3. Puxar dados nutricionais da Base de Dados
        var ingredientIds = lines.Select(l => l.IngredientId).ToList();
        var ingredients = await _context.Ingredients
            .Include(i => i.NutritionalInfo)
            .Where(i => ingredientIds.Contains(i.Id) && i.TenantId == TenantId)
            .ToListAsync();

        var solverIngredients = lines.Select(line =>
        {
            var ing = ingredients.First(i => i.Id == line.IngredientId);
            return new SolverIngredientInput(
                IngredientId: ing.Id.ToString(), Code: ing.Code, CostPerKg: ing.PriceCurrent,
                MinInclusionPercent: line.InclusionMinPercent?.Value, MaxInclusionPercent: line.InclusionMaxPercent?.Value, FixedInclusionPercent: line.FixedInclusionPercent?.Value
            );
        }).ToList();

        var nutrientProfiles = ingredients.SelectMany(ing => ing.NutritionalInfo.Select(ni => new SolverNutrientProfileInput(
            IngredientId: ing.Id.ToString(), NutrientId: ni.NutrientId.ToString(), Value: ni.Value
        ))).ToList();

        // 4. A MAGIA DAS RESTRIÇÕES (Juntar Globais com Ad-Hoc)
        var constraints = new List<SolverConstraintInput>();

        // A) Restrições do Set (Globais da Fazenda)
        if (dto.ConstraintSetId.HasValue)
        {
            var rules = await _context.ConstraintRules.Where(r => r.ConstraintSetId == dto.ConstraintSetId.Value).ToListAsync();
            constraints.AddRange(rules.Select(r => new SolverConstraintInput(
                Type: r.Type.ToString().ToLower(), NutrientId: r.NutrientId?.ToString(), MinValue: r.MinValue, MaxValue: r.MaxValue
            )));
        }

        // B) Restrições Manuais (Customizadas para este lote apenas!)
        if (dto.AdHocConstraints != null)
        {
            constraints.AddRange(dto.AdHocConstraints.Select(c => new SolverConstraintInput(
                Type: c.Type.ToLower(), NutrientId: c.NutrientId?.ToString(), MinValue: c.MinValue, MaxValue: c.MaxValue
            )));
        }

        // 5. Preparar o Pedido com o OBJETIVO DINÂMICO
        var request = new SolverProblemRequest(
            TenantId: TenantId.ToString(),
            FormulaVersionId: version.Id.ToString(),
            TargetBatchSizeKg: dto.TargetBatchSizeKg,
            Objective: dto.OptimizationObjective ?? "min_cost", // Agora o Nutricionista escolhe!
            Ingredients: solverIngredients,
            Constraints: constraints,
            NutrientProfiles: nutrientProfiles
        );

        // 6. Registar a Tentativa e Enviar para o Python
        var requestJson = JsonSerializer.Serialize(request);
        var solverRun = new FeedFormulation.Domain.Entities.Solver.SolverRun(TenantId, version.Id, _currentUser.GetUserId(), requestJson);
        solverRun.MarkRunning();
        _context.SolverRuns.Add(solverRun);
        await _context.SaveChangesAsync();

        SolverProblemResponse response;
        try { response = await _solverClient.SolveAsync(request); }
        catch (Exception ex)
        {
            solverRun.MarkFailed(ex.Message);
            await _context.SaveChangesAsync();
            return StatusCode(502, new { message = "Python Solver inacessível.", error = ex.Message });
        }

        // 7. Guardar o Resultado
        var responseJson = JsonSerializer.Serialize(response);
        if (response.Status == "succeeded") solverRun.MarkSucceeded(responseJson);
        else if (response.Status == "infeasible") solverRun.MarkInfeasible(response.DiagnosticMessage ?? "A matemática não fecha.");
        else solverRun.MarkFailed(response.DiagnosticMessage ?? "Erro.");

        await _context.SaveChangesAsync();

        // 8. Devolver tudo ao React!
        return Ok(new
        {
            formulaId = formula.Id,
            versionId = version.Id,
            solverRunId = solverRun.Id,
            status = response.Status,
            totalCost = response.TotalCost,
            results = response.IngredientResults,
            nutrients = response.NutrientResults,
            message = response.DiagnosticMessage
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var formula = await _context.Formulas.FirstOrDefaultAsync(f => f.Id == id && f.TenantId == TenantId);
        if (formula == null) return NotFound("Fórmula não encontrada.");

        _context.Formulas.Remove(formula);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Fórmula apagada." });
    }
}

// ==========================================
// --- OS NOVOS DTOs SUPER PODEROSOS ---
// ==========================================

public record AdvancedFormulaDto(
    string Name,
    string? Description,
    decimal TargetBatchSizeKg,
    Guid? ConstraintSetId,
    string? Notes,
    string? OptimizationObjective, // Novidade: "min_cost", "max_protein", etc!
    List<CreateLineDto> Lines,
    List<AdHocConstraintDto>? AdHocConstraints // Novidade: Regras na hora!
);

public record CreateLineDto(Guid IngredientId, decimal? MinPercent, decimal? MaxPercent, decimal? FixedPercent);

public record AdHocConstraintDto(string Type, Guid? NutrientId, decimal? MinValue, decimal? MaxValue);