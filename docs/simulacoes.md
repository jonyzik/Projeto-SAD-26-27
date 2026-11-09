# Módulo de Simulações e Custos Operacionais

Branch: `dev/simulacoes`. Cobre o requisito RF6 — «Perform a new simulation»: adoção do método AHP, cálculo dos custos operacionais e pontos fortes e fracos.

## Fluxo

1. **Simulações** (menu lateral) lista as simulações guardadas, com o parque recomendado, o custo mensal e a razão de consistência. Permite abrir, duplicar, eliminar e criar.
2. **Nova simulação** reúne, numa só página:
   1. Identificação (nome e descrição).
   2. Necessidades da empresa: área (m²), colaboradores, duração do contrato (meses), orçamento mensal máximo e localização pretendida (opcionais).
   3. Parâmetros dos custos operacionais, com «Repor predefinição», «Guardar como predefinição» e «Valores de origem».
   4. Seleção dos parques a comparar (mínimo 2), com o custo mensal estimado de cada um.
   5. Comparação par a par dos 9 critérios (36 pares) na escala de Saaty, com pesos e razão de consistência recalculados em tempo real e a opção de marcar critérios como obrigatórios.
   6. Execução e resultados: ranking, prioridade AHP, custos, motivos de exclusão, pontos fortes e fracos e contribuição de cada critério.
3. Depois de executada, a simulação pode ser guardada. Reabrir uma simulação mostra os resultados guardados; executá-la de novo cria uma simulação nova.

## Custos operacionais

Calculados em `OperationalCostCalculator` (valores mensais em euros, arredondados a 2 casas):

| Componente | Fórmula |
| --- | --- |
| Renda | área × preço por m² do parque |
| Condomínio e serviços comuns | área × valor por m² |
| Energia e água | área × valor por m² |
| Serviços por colaborador | colaboradores × valor por colaborador |
| Estacionamento | ⌈colaboradores × % com estacionamento⌉ × custo por lugar |
| Contingência | % sobre a soma anterior |
| **Total mensal** | soma de todas as componentes |
| Total anual | total mensal × 12 |
| Instalação (único) | área × custo por m² |
| **Total do contrato** | total mensal × meses + instalação |

Valores de origem: 2,50 €/m² de condomínio, 1,80 €/m² de energia e água, 35 € por colaborador, 50 % dos colaboradores com estacionamento a 40 €/lugar, 25 €/m² de instalação e 5 % de contingência. Os parâmetros predefinidos alterados pelo utilizador ficam em `%APPDATA%\ParkWise\simulation-parameters.json`.

## Integração com o AHP

- O contrato `IAhpEngine` (em `ParkWise.Application/Ahp`) define o que as simulações pedem ao algoritmo: `CalculatePriorities` (pesos, λmax, IC e RC de uma matriz par a par) e `Synthesize` (prioridade global das alternativas).
- `EigenvectorAhpEngine` é a implementação de referência usada até à integração do módulo `dev/ahp`: vetor próprio principal pelo método das potências e índices aleatórios de Saaty. Para usar o motor da equipa, basta registá-lo como `IAhpEngine` antes de `AddParkWiseInfrastructure` (é usado `TryAddSingleton`).
- Hierarquia: objetivo → 9 critérios → parques elegíveis.
  - Pesos dos critérios: matriz 9×9 construída a partir das comparações (`PairwiseComparisonMatrix`). Pares não indicados valem 1 (igual importância).
  - Prioridades locais dos parques: para os 8 critérios qualitativos, as pontuações do parque são normalizadas entre as alternativas (pontuação ÷ soma). Para o critério «Preço competitivo da área alugada», usa-se o **custo operacional mensal calculado** (1/custo, normalizado), pelo que os custos fazem parte do modelo de decisão.
  - Prioridade global = Σ peso do critério × prioridade local; empate desfeito pelo menor custo mensal.
- A simulação só é executada com RC ≤ 0,10.

## Elegibilidade

Um parque é excluído do ranking (e o motivo é apresentado) quando: não tem preço por m² válido; não tem avaliação para um dos 8 critérios qualitativos; tem pontuação 0 num critério marcado como obrigatório; o custo mensal ultrapassa o orçamento; ou fica fora da localização pretendida.

## Pontos fortes e fracos

Para cada parque elegível, compara-se a prioridade local em cada critério com a média das alternativas (1/n), ponderada pelo peso do critério. Os três maiores desvios positivos são pontos fortes; os três maiores negativos são pontos fracos. Com um único parque elegível não há comparação possível.

## Persistência

- `Simulation`: nome, descrição, data, utilizador (opcional até à integração de `dev/auth`), razão de consistência, necessidades da empresa (`CompanyRequirements`) e parâmetros de custo (`OperationalCostSettings`), estes dois como tipos *owned* na mesma tabela.
- `SimulationCriterionWeight`: peso de cada critério e indicação de obrigatório.
- `AhpComparison`: comparações não neutras indicadas pelo utilizador.
- `SimulationResult`: ranking (0 se excluído), prioridade, elegibilidade, motivo de exclusão, custos mensal, de instalação e total, pontos fortes e fracos.
- Sem ligação SQL Server, `DemoSimulationDataSource` usa parques fictícios e guarda as simulações em memória.

**É necessária uma migração** para as alterações ao modelo: novas colunas em `Simulations` e `SimulationResults`, `IsMandatory` em `SimulationCriterionWeights` e `UserId` opcional. Criar com `dotnet ef migrations add Simulacoes` quando a equipa decidir a primeira migração partilhada.

## Pontos de integração com a equipa

| Módulo | O que é preciso |
| --- | --- |
| dev/ahp | Implementar `IAhpEngine` (ou adaptar o motor existente a esse contrato) e registá-lo na DI. |
| dev/parques | As pontuações dos critérios vêm de `ParkEvaluation.Score` (qualquer escala positiva; 0 = serviço inexistente). O critério 9 não precisa de pontuação: é calculado a partir do preço. |
| dev/auth | Implementar `ICurrentUserAccessor` para associar as simulações ao utilizador autenticado. |
| dev/resultados | `ISimulationService.GetAsync` devolve `SimulationOutcome` com ranking, custos e pontos fortes/fracos, pronto para gráficos e exportação. |
