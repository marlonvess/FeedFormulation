using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.Enums;

public enum ReproductionEventType
{
    HeatObservation = 0, // Observação de Celo
    Insemination = 1,    // Inseminação Artificial / Cobrição
    PregnancyCheck = 2,  // Diagnóstico de Gestação (Toque/Ecografia)
    Calving = 3,         // Parto
    Abortion = 4         // Aborto
}
