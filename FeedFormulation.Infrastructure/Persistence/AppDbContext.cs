using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeedFormulation.Domain.Entities.Catalog;
using FeedFormulation.Domain.Entities.Formulation;
using FeedFormulation.Domain.Entities.Solver;
using FeedFormulation.Domain.Common;
using Microsoft.EntityFrameworkCore;
using FeedFormulation.Domain.Entities.Livestock;

namespace FeedFormulation.Infrastructure.Persistence;

/// <summary>
/// Context to manage the database connection and map the domain entities to database tables using Entity Framework Core.
/// </summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // --- Tables ---

    // Catalog management
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Nutrient> Nutrients => Set<Nutrient>();
    public DbSet<IngredientNutrientProfile> IngredientNutrientProfiles => Set<IngredientNutrientProfile>();

    
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<IngredientPrice> IngredientPrices => Set<IngredientPrice>();
    public DbSet<IngredientGroup> IngredientGroups => Set<IngredientGroup>();
    public DbSet<IngredientGroupMember> IngredientGroupMembers => Set<IngredientGroupMember>();
    public DbSet<ConstraintSet> ConstraintSets => Set<ConstraintSet>();
    public DbSet<ConstraintRule> ConstraintRules => Set<ConstraintRule>();

    // Formulation management
    public DbSet<Formula> Formulas => Set<Formula>();
    public DbSet<FormulaVersion> FormulaVersions => Set<FormulaVersion>();
    public DbSet<FormulaLine> FormulaLines => Set<FormulaLine>();
    public DbSet<SolverRun> SolverRuns => Set<SolverRun>();
    public DbSet<SolverRunLineResult> SolverRunLineResults => Set<SolverRunLineResult>();
    public DbSet<SolverRunNutrientResult> SolverRunNutrientResults => Set<SolverRunNutrientResult>();

    /// Animal management
    public DbSet<Animal> Animals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Aplica as configurações (tamanhos de campos, chaves) automaticamente
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}