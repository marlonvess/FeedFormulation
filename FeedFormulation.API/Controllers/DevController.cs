using FeedFormulation.Application.Services;
using FeedFormulation.Domain.Dtos.Solver; 
using Microsoft.AspNetCore.Mvc;


namespace FeedFormulation.Api.Controllers;

/// <summary>
/// Provides API endpoints for development and testing operations related to formula calculations for bovine samples.
/// </summary>
/// <remarks>This controller is intended for use in a web API context and exposes endpoints that utilize the
/// FormulaService to solve sample problems. The FormulaService instance is injected automatically via dependency
/// injection, as configured in the application's startup. Endpoints are designed for development purposes and may
/// return error details for troubleshooting.</remarks>
[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    /// <summary>
    /// Provides access to the FormulaService instance used for formula-related operations within this class.
    /// </summary>
    private readonly FormulaService _formulaService;

    // O .NET injeta o FormulaService automaticamente aqui porque configuramos no Program.cs
    public DevController(FormulaService formulaService)
    {
        _formulaService = formulaService;
    }
    /// <summary>
    /// Handles a POST request to solve a sample bovine feed formulation problem and returns the result.
    /// </summary>
    /// <remarks>This endpoint is intended for testing or demonstration purposes and invokes the sample
    /// formula creation logic. The response includes the status and details of the created formula. If the operation
    /// fails, the response will contain failure information.</remarks>
    /// <returns>An ActionResult containing the response of the solver problem. Returns a BadRequest if the formula creation
    /// fails, or a 500 status code with error details if an unexpected error occurs.</returns>
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