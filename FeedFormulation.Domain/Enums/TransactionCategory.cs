using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.Enums;

public enum TransactionCategory
{
    MilkSales = 1,       // Venda de Leite
    AnimalSales = 2,     // Venda de Animais
    FeedAndNutrition = 3,// Alimentação / Ração
    Veterinary = 4,      // Veterinário / Medicamentos
    Salaries = 5,        // Salários / Mão de obra
    Equipment = 6,       // Máquinas / Manutenção
    Other = 7            // Outros / Diversos
}
