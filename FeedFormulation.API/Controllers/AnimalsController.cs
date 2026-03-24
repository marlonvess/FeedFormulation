using FeedFormulation.Api.Services;
using FeedFormulation.Domain.Entities.Livestock;
using FeedFormulation.Domain.Enums;
using FeedFormulation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnimalsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AnimalsController(AppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    private Guid TenantId => _currentUser.GetTenantId();

    [HttpPost]
    [Authorize(Roles = "FarmWorker, Veterinarian, Manager, Admin")] // 🔒 Todos menos o Nutricionista podem inserir
    public async Task<IActionResult> Create([FromBody] CreateAnimalDto dto)
    {
        var animal = new Animal(TenantId, dto.SiaNumber, dto.Name, dto.InternalNumber, dto.DateOfBirth, dto.Gender, dto.Breed);
        _context.Animals.Add(animal);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Animal successfully registered!", id = animal.Id });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // 1. Puxa da base de dados primeiro
        var dbAnimals = await _context.Animals
            .Where(a => a.TenantId == TenantId && a.Status == AnimalStatus.Active)
            .ToListAsync();

        // 2. Formata para o React na memória
        var animals = dbAnimals.Select(a => new
        {
            a.Id,
            a.SiaNumber,
            a.Name,
            a.InternalNumber,
            a.DateOfBirth,
            Age = $"{a.GetAgeInMonths() / 12}Y {a.GetAgeInMonths() % 12}M",
            Gender = a.Gender.ToString(), // <-- Corrigido o erro do a.Gender
            a.Breed,
            Group = a.Group ?? "-",
            Lot = a.Lot ?? "-",
            a.LastMilkProduction
        });

        return Ok(animals);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id && a.TenantId == TenantId);
        if (animal == null) return NotFound();
        return Ok(animal);
    }

    [HttpPut("{id}/lot")]
    [Authorize(Roles = "FarmWorker, Manager, Admin")]
    public async Task<IActionResult> AssignLot(Guid id, [FromBody] string lot)
    {
        var animal = await _context.Animals.FirstOrDefaultAsync(a => a.Id == id && a.TenantId == TenantId);
        if (animal == null) return NotFound();
        animal.AssignToLot(lot);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Lot updated." });
    }
}

public record CreateAnimalDto(string SiaNumber, string Name, string InternalNumber, DateTime DateOfBirth, AnimalGender Gender, string Breed);