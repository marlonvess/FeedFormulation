using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.Enums;
/// <summary>
///
/// </summary>
public enum SolverRunStatus
{
    Queued,     // Na fila
    Running,    // Calculando
    Succeeded,  // Sucesso
    Failed,     // Erro técnico
    Infeasible  // Impossível resolver matematicamente
}