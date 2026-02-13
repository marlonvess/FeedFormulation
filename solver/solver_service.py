from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from typing import List, Optional, Dict
import pyomo.environ as pyo

app = FastAPI(title="Feed Formulation Solver", version="1.0")

# -----------------------------
# DTOs (Modelos de Dados)
# -----------------------------
class SolverIngredientInput(BaseModel):
    ingredientId: str
    code: str
    costPerKg: float
    minInclusionPercent: Optional[float] = None
    maxInclusionPercent: Optional[float] = None
    fixedInclusionPercent: Optional[float] = None

class SolverNutrientProfileInput(BaseModel):
    ingredientId: str
    nutrientId: str
    value: float

class SolverConstraintInput(BaseModel):
    type: str  # nutrient_min, nutrient_max, ingredient_min, ingredient_max, group_sum_min, group_sum_max
    nutrientId: Optional[str] = None
    ingredientId: Optional[str] = None
    ingredientGroupId: Optional[str] = None
    ingredientIds: Optional[List[str]] = None  # expansão do grupo (IDs dos ingredientes que pertencem ao grupo)
    minValue: Optional[float] = None
    maxValue: Optional[float] = None

class SolverProblemRequest(BaseModel):
    tenantId: str
    formulaVersionId: str
    targetBatchSizeKg: float
    objective: str
    ingredients: List[SolverIngredientInput]
    constraints: List[SolverConstraintInput]
    nutrientProfiles: List[SolverNutrientProfileInput]

class SolverIngredientResult(BaseModel):
    ingredientId: str
    inclusionPercent: float
    costContribution: float

class SolverNutrientResult(BaseModel):
    nutrientId: str
    achievedValue: float
    minRequired: Optional[float] = None
    maxAllowed: Optional[float] = None
    isBinding: bool

class SolverProblemResponse(BaseModel):
    status: str  # succeeded | infeasible | failed
    solverName: str
    totalCost: float
    ingredientResults: List[SolverIngredientResult]
    nutrientResults: List[SolverNutrientResult]
    diagnosticMessage: Optional[str] = None

# -----------------------------
# Lógica do Solver
# -----------------------------
@app.post("/solve", response_model=SolverProblemResponse)
def solve(req: SolverProblemRequest):
    if req.objective != "min_cost":
        raise HTTPException(status_code=400, detail="Só 'min_cost' é suportado nesta fase.")

    # Lista única de IDs de ingredientes
    ing_ids = [i.ingredientId for i in req.ingredients]
    if len(set(ing_ids)) != len(ing_ids):
        raise HTTPException(status_code=400, detail="Ingredientes repetidos no request.")

    # 1. Mapas de Dados para acesso rápido
    cost = {i.ingredientId: float(i.costPerKg) for i in req.ingredients}
    min_inc = {}
    max_inc = {}
    fixed_inc = {}

    for i in req.ingredients:
        if i.fixedInclusionPercent is not None:
            fixed_inc[i.ingredientId] = float(i.fixedInclusionPercent)
        else:
            if i.minInclusionPercent is not None:
                min_inc[i.ingredientId] = float(i.minInclusionPercent)
            if i.maxInclusionPercent is not None:
                max_inc[i.ingredientId] = float(i.maxInclusionPercent)

    # Matriz Nutricional: (ingredientId, nutrientId) -> valor
    nut = {}
    for p in req.nutrientProfiles:
        nut[(p.ingredientId, p.nutrientId)] = float(p.value)

    # Identificar quais nutrientes estão a ser controlados (para reportar no final)
    used_nutrients = []
    for c in req.constraints:
        if c.type.startswith("nutrient_") and c.nutrientId:
            used_nutrients.append(c.nutrientId)
    used_nutrients = sorted(set(used_nutrients))

    # 2. Construção do Modelo Matemático (Pyomo)
    m = pyo.ConcreteModel()
    m.I = pyo.Set(initialize=ing_ids)

    # Variáveis de Decisão: x[i] é a % de inclusão (0 a 100)
    m.x = pyo.Var(m.I, domain=pyo.NonNegativeReals, bounds=(0.0, 100.0))

    # Restrição Fundamental: Soma tem de ser 100%
    def sum_100_rule(mm):
        return sum(mm.x[i] for i in mm.I) == 100.0
    m.sum100 = pyo.Constraint(rule=sum_100_rule)

    # Aplicar Limites Individuais (Fixo, Min, Max)
    m.fixed = pyo.ConstraintList()
    for i, v in fixed_inc.items():
        m.fixed.add(m.x[i] == v)

    m.bounds = pyo.ConstraintList()
    for i, v in min_inc.items():
        m.bounds.add(m.x[i] >= v)
    for i, v in max_inc.items():
        m.bounds.add(m.x[i] <= v)

    # 3. Aplicar Restrições de Negócio (Nutrientes e Grupos)
    m.cons = pyo.ConstraintList()
    
    # Dicionários auxiliares para o report final
    nut_min_req: Dict[str, float] = {}
    nut_max_all: Dict[str, float] = {}

    for c in req.constraints:
        t = c.type
        
        # Mínimo de Nutriente
        if t == "nutrient_min":
            if not c.nutrientId or c.minValue is None: continue
            nId = c.nutrientId
            nut_min_req[nId] = float(c.minValue)
            # Regra: soma(x[i] * valor_nutriente) / 100 >= minimo
            m.cons.add(sum(m.x[i] * nut.get((i, nId), 0.0) for i in m.I) / 100.0 >= float(c.minValue))

        # Máximo de Nutriente
        elif t == "nutrient_max":
            if not c.nutrientId or c.maxValue is None: continue
            nId = c.nutrientId
            nut_max_all[nId] = float(c.maxValue)
            m.cons.add(sum(m.x[i] * nut.get((i, nId), 0.0) for i in m.I) / 100.0 <= float(c.maxValue))

        # Mínimo de Ingrediente (redundante com bounds, mas útil se vier como regra separada)
        elif t == "ingredient_min":
            if c.ingredientId and c.minValue is not None and c.ingredientId in ing_ids:
                m.cons.add(m.x[c.ingredientId] >= float(c.minValue))

        # Máximo de Ingrediente
        elif t == "ingredient_max":
            if c.ingredientId and c.maxValue is not None and c.ingredientId in ing_ids:
                m.cons.add(m.x[c.ingredientId] <= float(c.maxValue))

        # Soma de Grupo (Min/Max)
        elif t in ("group_sum_min", "group_sum_max"):
            ids = c.ingredientIds or []
            # Filtra apenas ingredientes que realmente existem no problema atual
            ids = [iid for iid in ids if iid in ing_ids]
            
            if not ids: continue # Se o grupo não tem ingredientes na fórmula, ignora

            if t == "group_sum_min" and c.minValue is not None:
                m.cons.add(sum(m.x[i] for i in ids) >= float(c.minValue))
            
            if t == "group_sum_max" and c.maxValue is not None:
                m.cons.add(sum(m.x[i] for i in ids) <= float(c.maxValue))

    # 4. Função Objetivo: Minimizar Custo
    # Custo Total = soma(x[i] * custo[i]) / 100
    def obj_rule(mm):
        return sum(mm.x[i] * cost[i] for i in mm.I) / 100.0
    m.obj = pyo.Objective(rule=obj_rule, sense=pyo.minimize)

    # 5. Execução do Solver (HiGHS)
    try:
        solver = pyo.SolverFactory("highs")
        res = solver.solve(m, tee=False)
    except Exception as ex:
        return SolverProblemResponse(
            status="failed", solverName="HiGHS", totalCost=0.0,
            ingredientResults=[], nutrientResults=[], diagnosticMessage=str(ex)
        )

    # Verificar Status da Resolução
    term = str(res.solver.termination_condition).lower()
    if "infeasible" in term:
        return SolverProblemResponse(
            status="infeasible", solverName="HiGHS", totalCost=0.0,
            ingredientResults=[], nutrientResults=[], 
            diagnosticMessage=f"Impossível resolver: {res.solver.termination_condition}"
        )

    if "optimal" not in term:
        return SolverProblemResponse(
            status="failed", solverName="HiGHS", totalCost=0.0,
            ingredientResults=[], nutrientResults=[], 
            diagnosticMessage=f"Solver terminou sem ótimo: {res.solver.termination_condition}"
        )

    # 6. Compilar Resultados
    # Extrair valores calculados das variáveis
    x_val = {i: float(pyo.value(m.x[i])) for i in ing_ids}
    
    total_cost_per_kg = sum((x_val[i] / 100.0) * cost[i] for i in ing_ids)
    total_cost_per_ton = total_cost_per_kg * 1000.0

    ingredient_results = []
    for i in ing_ids:
        inclusion = x_val[i]
        # Contribuição para o custo (por tonelada)
        contrib = (inclusion / 100.0) * cost[i] * 1000.0
        ingredient_results.append(SolverIngredientResult(
            ingredientId=i,
            inclusionPercent=round(inclusion, 4),
            costContribution=round(contrib, 6)
        ))

    nutrient_results = []
    for nId in used_nutrients:
        # Calcular valor atingido do nutriente
        achieved = sum((x_val[i] / 100.0) * nut.get((i, nId), 0.0) for i in ing_ids)
        
        min_req = nut_min_req.get(nId)
        max_all = nut_max_all.get(nId)
        
        # Verificar se a restrição está "ativa" (binding)
        is_binding = False
        eps = 1e-4
        if min_req is not None and abs(achieved - min_req) <= eps: is_binding = True
        if max_all is not None and abs(achieved - max_all) <= eps: is_binding = True

        nutrient_results.append(SolverNutrientResult(
            nutrientId=nId,
            achievedValue=round(achieved, 6),
            minRequired=min_req,
            maxAllowed=max_all,
            isBinding=is_binding
        ))

    return SolverProblemResponse(
        status="succeeded",
        solverName="HiGHS",
        totalCost=round(total_cost_per_ton, 6),
        ingredientResults=ingredient_results,
        nutrientResults=nutrient_results
    )