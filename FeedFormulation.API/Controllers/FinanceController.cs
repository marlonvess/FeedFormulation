using FeedFormulation.Domain.Entities.Finance;
using FeedFormulation.Domain.Enums;
using FeedFormulation.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FeedFormulation.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinanceController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly Guid _tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public FinanceController(AppDbContext context)
    {
        _context = context;
    }

    // 1. POST: Regista uma nova transação (Receita ou Despesa)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionDto dto)
    {
        var transaction = new FinancialTransaction(
            _tenantId, dto.Date, dto.Amount, dto.Type, dto.Category, dto.Description, dto.AnimalId
        );

        _context.FinancialTransactions.Add(transaction);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Transaction successfully registered!", id = transaction.Id });
    }

    // 2. GET: Histórico de Transações (Extrato estilo Conta Bancária)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var transactions = await _context.FinancialTransactions
            .Where(t => t.TenantId == _tenantId && !t.IsDeleted)
            .OrderByDescending(t => t.Date)
            .Select(t => new
            {
                id = t.Id,
                date = t.Date.ToString("yyyy-MM-dd"),
                amount = t.Amount,
                type = t.Type.ToString(),
                category = t.Category.ToString(),
                description = t.Description,
                animalId = t.AnimalId
            })
            .ToListAsync();

        return Ok(transactions);
    }

    // ==========================================
    // --- BUSINESS INTELLIGENCE (BI) ---
    // ==========================================

    // 3. GET: Resumo Financeiro Mensal (Para o Grande Dashboard!)
    [HttpGet("monthly-summary")]
    public async Task<IActionResult> GetMonthlySummary([FromQuery] int? year, [FromQuery] int? month)
    {
        // Se não passarmos ano/mês no Swagger, ele assume o mês atual
        var targetYear = year ?? DateTime.UtcNow.Year;
        var targetMonth = month ?? DateTime.UtcNow.Month;

        // Puxa as transações do mês escolhido para a memória
        var transactions = await _context.FinancialTransactions
            .Where(t => t.TenantId == _tenantId && !t.IsDeleted && t.Date.Year == targetYear && t.Date.Month == targetMonth)
            .ToListAsync();

        // Faz os cálculos da contabilidade
        var totalRevenue = transactions.Where(t => t.Type == TransactionType.Revenue).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var profit = totalRevenue - totalExpense;

        // Agrupa as despesas por categoria (Perfeito para um Gráfico Circular no React)
        var expensesByCategory = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .GroupBy(t => t.Category)
            .Select(g => new
            {
                category = g.Key.ToString(),
                totalAmount = Math.Round(g.Sum(t => t.Amount), 2)
            })
            .OrderByDescending(c => c.totalAmount) // Ordena da maior despesa para a menor
            .ToList();

        return Ok(new
        {
            Period = $"{targetYear}-{targetMonth:D2}",
            TotalRevenue = Math.Round(totalRevenue, 2),
            TotalExpense = Math.Round(totalExpense, 2),
            NetProfit = Math.Round(profit, 2), // Lucro Líquido
            ExpensesByCategory = expensesByCategory
        });
    }

    // ==========================================
    // --- SOFT DELETE ---
    // ==========================================

    // 4. DELETE: Apagar transação incorreta
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var transaction = await _context.FinancialTransactions
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == _tenantId && !t.IsDeleted);

        if (transaction == null) return NotFound("Transaction not found or already deleted.");

        transaction.MarkAsDeleted();
        await _context.SaveChangesAsync();

        return Ok(new { message = "Transaction deleted (Soft Delete)." });
    }
}

// ==========================================
// --- DTO ---
// ==========================================

public record CreateTransactionDto(
    DateTime Date,
    decimal Amount,
    TransactionType Type, // 1 = Receita, 2 = Despesa
    TransactionCategory Category, // 1 a 7 (Leite, Animais, Ração, Vet, Salários, Equipamentos, Outros)
    string Description,
    Guid? AnimalId // Opcional (Se foi venda de um bezerro específico, por exemplo)
);