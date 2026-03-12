using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedFormulation.Domain.Enums;

public enum HealthEventType
{
    Vaccination = 0,  // Vacinação
    Illness = 1,      // Doença / Diagnóstico Veterinário
    Treatment = 2,    // Tratamento / Medicação
    HoofTrimming = 3, // Podologia (Corte de unhas)
    Checkup = 4       // Exame de Rotina
}