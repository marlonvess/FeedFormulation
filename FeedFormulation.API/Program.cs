using FeedFormulation.Infrastructure.Persistence;
using FeedFormulation.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

builder.Services.AddControllers();

// Configuração do Solver Python
builder.Services.AddHttpClient<FeedFormulation.Infrastructure.Http.SolverHttpClient>();

builder.Services.AddScoped<FeedFormulation.Application.Services.FormulaService>();
// Configure the HTTP request pipeline.

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();

        // 1. Aplica Migrations automaticamente (cria o banco se não existir)
        await context.Database.MigrateAsync();

        // 2. Definimos um ID fixo para a nossa "Fazenda de Teste"
        // (Mais tarde, isto virá do login do utilizador)
        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // 3. Executa os Seeds na ordem correta
        Console.WriteLine("A iniciar o Seed do banco de dados...");

        await BovineBaselineSeed.SeedAsync(context, tenantId);
        await BovineIngredientSeed.SeedAsync(context, tenantId);
        await BovineConstraintSetsSeed.SeedAsync(context, tenantId);

        Console.WriteLine("Banco de dados semeado com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Erro ao criar/semear o banco: {ex.Message}");
    }
}



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
