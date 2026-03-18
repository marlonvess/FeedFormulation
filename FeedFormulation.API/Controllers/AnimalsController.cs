using FeedFormulation.Domain.Entities.Livestock;
using FeedFormulation.Domain.Enums;
using FeedFormulation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public AnimalsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. GET: Return a list of all active animals in the herd, ordered by SIA Number
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var animals = await _context.Animals
            .Where(a => a.TenantId == _tenantId && a.Status == AnimalStatus.Active)
            .OrderBy(a => a.SiaNumber)
            .ToListAsync();

        var result = animals.Select(a => new
        {
            id = a.Id,
            siaNumber = a.SiaNumber,
            name = a.Name ?? "Unnamed",
            internalNumber = a.InternalNumber ?? "-",
            age = CalculateAge(a.DateOfBirth), // Retorna "4Y 2M" (Years/Months)
            gender = a.Gender.ToString(),
            breed = a.Breed,
            group = a.Group ?? "-",
            lot = a.Lot ?? "-",
            lastMilkProduction = a.LastMilkProduction,
            dateOfBirth = a.DateOfBirth.ToString("yyyy-MM-dd")
        });

        return Ok(result);
    }

    // 2. POST: Register a new animal in the herd, ensuring that the SIA Number is unique within the tenant's herd
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAnimalDto dto)
    {
        var exists = await _context.Animals.AnyAsync(a => a.TenantId == _tenantId && a.SiaNumber == dto.SiaNumber);
        if (exists) return BadRequest("An animal with this SIA Number already exists in the herd.");

        var animal = new Animal(
            _tenantId,
            dto.SiaNumber,
            dto.Name,
            dto.InternalNumber,
            dto.DateOfBirth,
            dto.Gender,
            dto.Breed
        );

        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Animal successfully registered!", id = animal.Id });
    }

    // 3. PUT: Update the management information of an existing animal (Group and Lot)
    [HttpPut("{id}/management")]
    public async Task<IActionResult> UpdateManagement(Guid id, [FromBody] UpdateManagementDto dto)
    {
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id && a.TenantId == _tenantId);
        if (animal == null) return NotFound("Animal not found.");

        animal.UpdateManagementInfo(dto.Group, dto.Lot);

        await _context.SaveChangesAsync();
        return Ok(new { message = "Management info successfully updated!" });
    }

    // 4. PUT: register the milk production of an animal for the current day, ensuring that the value is non-negative and updating the LastMilkProduction property of the animal
    [HttpPut("{id}/production")]
    public async Task<IActionResult> UpdateProduction(Guid id, [FromBody] decimal milkProduction)
    {
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id && a.TenantId == _tenantId);
        if (animal == null) return NotFound("Animal not found.");

        animal.UpdateProduction(milkProduction);

        await _context.SaveChangesAsync();
        return Ok(new { message = "Production successfully updated!" });
    }

    // 5. DELETE: delete an animal from the herd by changing its status to "Sold" or "Deceased" (the endpoint should accept a query parameter to specify the new status, and only allow changing to "Sold" if the current status is "Active")
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] AnimalStatus status)
    {
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id && a.TenantId == _tenantId);
        if (animal == null) return NotFound("Animal not found.");

        animal.ChangeStatus(status == AnimalStatus.Active ? AnimalStatus.Sold : status);

        await _context.SaveChangesAsync();
        return Ok(new { message = $"Animal status changed to {animal.Status}." });
    }

    // --- Helper Method ---
    private static string CalculateAge(DateTime dateOfBirth)
    {
        var today = DateTime.Today;
        int months = (today.Year - dateOfBirth.Year) * 12 + today.Month - dateOfBirth.Month;
        if (today.Day < dateOfBirth.Day) months--;

        int years = months / 12;
        int remainingMonths = months % 12;

        return $"{years}Y {remainingMonths}M"; // Y = Years, M = Months
    }
}

// --- DTOs ---
public record CreateAnimalDto(
    string SiaNumber,
    string? Name,
    string? InternalNumber,
    DateTime DateOfBirth,
    AnimalGender Gender,
    string Breed
);

public record UpdateManagementDto(string? Group, string? Lot);