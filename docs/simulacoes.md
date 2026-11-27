# Módulo de Simulações e Custos Operacionais

Branch: `dev/simulacoes`. Cobre o requisito RF6 — «Perform a new simulation»: adoção do método AHP, cálculo dos custos operacionais e pontos fortes e fracos.

## Fluxo

1. **Simulações** (menu lateral) lista as simulações guardadas, com o parque recomendado, o custo mensal e a razão de consistência. Permite abrir, duplicar, eliminar e criar.
2. **Nova simulação** reúne, numa só página:
   1. Identificação (nome e descrição).
   2. Área e contrato: área necessária (m²) e duração do contrato (meses), usadas no cálculo dos custos operacionais.
   3. Seleção dos parques a comparar (mínimo 2), com o custo mensal estimado de cada um.
   4. Importância de cada um dos 9 critérios numa escala de 1 (mínima) a 9 (máxima), com os pesos AHP recalculados em tempo real e a opção de marcar critérios como obrigatórios.
   5. Execução e resultados: ranking, prioridade AHP, custos operacionais, motivos de exclusão, pontos fortes e fracos e contribuição de cada critério.
3. Depois de executada, a simulação pode ser guardada. Reabrir uma simulação mostra os resultados guardados; executá-la de novo cria uma simulação nova.

## Custos operacionais

Calculados em `OperationalCostCalculator`, em euros:

| Custo | Fórmula |
| --- | --- |
| Mensal | área (m²) × preço por m² do parque, arredondado ao cêntimo |
| Anual | custo mensal × 12 |
| Total do contrato | custo mensal × duração do contrato (meses) |

## Integração com o AHP

- O contrato `IAhpEngine` (em `ParkWise.Application/Ahp`) define o que as simulações pedem ao algoritmo: `CalculatePriorities` (pesos, λmax, IC e RC de uma matriz par a par) e `Synthesize` (prioridade global das alternativas).
- `EigenvectorAhpEngine` é a implementação de referência usada até à integração do módulo `dev/ahp`: vetor próprio principal pelo método das potências e índices aleatórios de Saaty. Para usar o motor da equipa, basta registá-lo como `IAhpEngine` antes de `AddParkWiseInfrastructure` (é usado `TryAddSingleton`).
- Hierarquia: objetivo → 9 critérios → parques elegíveis.
  - Pesos dos critérios: o utilizador dá a cada critério uma importância de 1 a 9 (`ImportanceScale`). A matriz 9×9 de comparação par a par é construída a partir dessas notas, com a_ij = importância_i / importância_j (`PairwiseComparisonMatrix.FromImportance`), e passa pelo motor AHP. Todas as razões ficam dentro da escala de Saaty (1/9 a 9); critérios sem nota assumem 5.
  - Prioridades locais dos parques: para os 8 critérios qualitativos, as pontuações do parque são normalizadas entre as alternativas (pontuação ÷ soma). Para o critério «Preço competitivo da área alugada», usa-se o **custo operacional mensal calculado** (1/custo, normalizado), pelo que os custos fazem parte do modelo de decisão.
  - Prioridade global = Σ peso do critério × prioridade local; empate desfeito pelo menor custo mensal.
- Como a matriz é construída a partir de notas, é sempre perfeitamente consistente (RC = 0) e os pesos ficam proporcionais às notas: cada peso é a nota do critério a dividir pela soma das nove notas. A comparação par a par dos 36 pares, com verificação de consistência, é tratada no módulo AHP (dev/ahp).

## Elegibilidade

Um parque é excluído do ranking (e o motivo é apresentado) quando: não tem preço por m² válido; não tem avaliação para um dos 8 critérios qualitativos; ou tem pontuação 0 num critério marcado como obrigatório.

## Pontos fortes e fracos

Para cada parque elegível, compara-se a prioridade local em cada critério com a média das alternativas (1/n), ponderada pelo peso do critério. Os três maiores desvios positivos são pontos fortes; os três maiores negativos são pontos fracos. Com um único parque elegível não há comparação possível.

## Persistência

- `Simulation`: nome, descrição, data, utilizador (opcional até à integração de `dev/auth`), razão de consistência, área e duração do contrato (`CompanyRequirements`, tipo *owned* na mesma tabela).
- `SimulationCriterionWeight`: importância (1 a 9) e peso de cada critério, e indicação de obrigatório.
- `SimulationResult`: ranking (0 se excluído), prioridade, elegibilidade, motivo de exclusão, custo mensal e custo total do contrato, pontos fortes e fracos.
- Sem ligação SQL Server, `DemoSimulationDataSource` usa parques fictícios e guarda as simulações em memória.

**É necessária uma migração** para as alterações ao modelo: novas colunas em `Simulations` e `SimulationResults`, `Importance` e `IsMandatory` em `SimulationCriterionWeights` e `UserId` opcional. Criar com `dotnet ef migrations add Simulacoes` quando a equipa decidir a primeira migração partilhada.

## Pontos de integração com a equipa

| Módulo | O que é preciso |
| --- | --- |
| dev/ahp | Implementar `IAhpEngine` (ou adaptar o motor existente a esse contrato) e registá-lo na DI. |
| dev/parques | As pontuações dos critérios vêm de `ParkEvaluation.Score` (qualquer escala positiva; 0 = serviço inexistente). O critério 9 não precisa de pontuação: é calculado a partir do preço. |
| dev/auth | Implementar `ICurrentUserAccessor` para associar as simulações ao utilizador autenticado. |
| dev/resultados | `ISimulationService.GetAsync` devolve `SimulationOutcome` com ranking, custos e pontos fortes/fracos, pronto para gráficos e exportação. |
