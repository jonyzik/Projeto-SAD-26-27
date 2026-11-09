namespace ParkWise.Application.Ahp;

/// <summary>
/// Contrato entre o módulo de simulações (dev/simulacoes) e o algoritmo AHP (dev/ahp).
/// O módulo de simulações constrói as matrizes e pede ao motor AHP os pesos e a síntese final.
/// </summary>
public interface IAhpEngine
{
    /// <summary>
    /// Calcula o vetor de prioridades (pesos) de uma matriz de comparação par a par
    /// quadrada e recíproca, bem como os indicadores de consistência.
    /// </summary>
    AhpPriorityResult CalculatePriorities(double[,] pairwiseMatrix);

    /// <summary>
    /// Combina os pesos dos critérios com as prioridades locais de cada alternativa
    /// (<paramref name="alternativePriorities"/>[alternativa, critério]) e devolve a prioridade global de cada alternativa.
    /// </summary>
    IReadOnlyList<double> Synthesize(IReadOnlyList<double> criteriaWeights, double[,] alternativePriorities);
}

public sealed record AhpPriorityResult(
    IReadOnlyList<double> Weights,
    double LambdaMax,
    double ConsistencyIndex,
    double ConsistencyRatio);
