using FeedFormulation.Application.Services;
using FeedFormulation.Domain.Dtos.Solver; 
using Microsoft.AspNetCore.Mvc;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    private readonly FormulaService _formulaService;

    // O .NET injeta o FormulaService automaticamente aqui porque configuramos no Program.cs
    public DevController(FormulaService formulaService)
    {
        _formulaService = formulaService;
    }

    [HttpPost("solve-sample")]
    public async Task<ActionResult<SolverProblemResponse>> SolveSample()
    {
        try
        {
            // Chama a lógica que criamos na Application
            var result = await _formulaService.CreateSampleBovineFormulaAsync();

            if (result.Status == "failed")
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, details = ex.InnerException?.Message });
        }
    }
}