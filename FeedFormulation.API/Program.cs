using FeedFormulation.Infrastructure.Persistence;
using FeedFormulation.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Configuração do Solver Python
builder.Services.AddHttpClient<FeedFormulation.Infrastructure.Http.SolverHttpClient>();

builder.Services.AddScoped<FeedFormulation.Application.Services.FormulaService>();
// Configure the HTTP request pipeline.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 1. Permitir que o React fale com a API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5173") // Portas normais do React/Vite
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

///  when it starts, we create a scope to access the services and ensure the database is created and seeded
var app = builder.Build();

app.UseCors("AllowReactApp"); // 2. Ativa a fronteira aberta para o React




///  creating a scope to access the registered services, such as AppDbContext, and perform migration and seeding operations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    { 
        var context = services.GetRequiredService<AppDbContext>();

        // 1. Apply migrations automatically (creates the database if it doesn't exist)
        /// Note: In production, consider using explicit migrations instead of automatic ones for better control over database changes.
        await context.Database.MigrateAsync();

        // 2. Define a fixed ID for our "Test Farm" tenant
        //  Later , this will come from the user login/session in a real application. For now, we use a hardcoded tenant ID to associate all seeded data with a specific tenant.
        /// Note: In a real application, tenant management would be more complex and secure, likely involving authentication and dynamic tenant resolution.
        var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // 3. running the seeders in the correct order to ensure that all necessary data is present before dependent data is seeded. For example, nutrients must be seeded before ingredients, and both must be seeded before constraint sets that reference them.
        /// Note: Ensure that seeds are idempotent and can be safely run multiple times without causing data duplication or integrity issues.
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

/// set up the middleware pipeline for the application, including Swagger for API documentation,
/// CORS policy, HTTPS redirection, and routing for controllers. The configuration is designed to allow for easy development and testing while also providing a foundation for secure and efficient API operations in production.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}



app.UseHttpsRedirection();


//// Apply CORS policy to allow cross-origin requests, which is essential for enabling frontend applications hosted on different domains to interact with this API. The "AllowAll" policy defined earlier permits any origin, 
///method, and header, making it suitable for development and testing. In production, consider implementing a more restrictive CORS policy to enhance security by allowing only trusted origins.
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
