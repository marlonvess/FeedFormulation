using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.Enums;

public enum DataSourceType
{
    Table,      // Tabela (ex: INRA, NRC)
    Lab,        // Análise de Laboratório
    Supplier,   // Fornecedor
    Manual      // Inserido à mão
}
