namespace ParkWise.Application.Simulations;

/// <summary>Guarda os parâmetros predefinidos usados quando se cria uma nova simulação.</summary>
public interface ISimulationParametersStore
{
    Task<SimulationParameters> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(SimulationParameters parameters, CancellationToken cancellationToken = default);
    Task ResetAsync(CancellationToken cancellationToken = default);
}
