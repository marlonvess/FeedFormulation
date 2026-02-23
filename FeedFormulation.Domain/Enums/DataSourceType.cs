using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.Enums;

/// <summary>
/// Defines the various types of data sources that can be used in feed formulation. 
/// This enumeration helps identify where the data for ingredients, nutritional values, and other relevant information is coming from, whether it's from established tables, laboratory analyses, suppliers, or manual entries.
/// </summary>
public enum DataSourceType
{
    Table,      // Tabela (ex: INRA, NRC)
    Lab,        // Análise de Laboratório
    Supplier,   // Fornecedor
    Manual      // Inserido à mão
}
