using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using FeedFormulation.Domain.Dtos.Solver;
using Microsoft.Extensions.Configuration;

namespace FeedFormulation.Infrastructure.Http;

/// <summary>
///  
/// </summary>
public class SolverHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly string _solverUrl;

    public SolverHttpClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        // Vamos buscar a URL ao ficheiro de configuração (appsettings.json)
        _solverUrl = configuration["SolverSettings:BaseUrl"] ?? "http://localhost:8001";
    }

    public async Task<SolverProblemResponse> SolveAsync(SolverProblemRequest request)
    {
        // Enviamos o POST para a rota /solve do Python
        var response = await _httpClient.PostAsJsonAsync($"{_solverUrl}/solve", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return new SolverProblemResponse("failed", "System", 0,
                new(), new(), $"Erro na comunicação com o Solver: {error}");
        }

        return await response.Content.ReadFromJsonAsync<SolverProblemResponse>()
               ?? throw new Exception("Falha ao processar resposta do Solver.");
    }
}