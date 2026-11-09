namespace ParkWise.App.Navigation;

/// <summary>
/// Passa a simulação a abrir entre a página «Simulações» e a página «Nova simulação»,
/// sem alterar o contrato de navegação partilhado.
/// </summary>
public sealed class SimulationSession
{
    private Guid? _pendingId;
    private bool _asCopy;

    public void RequestOpen(Guid simulationId, bool asCopy)
    {
        _pendingId = simulationId;
        _asCopy = asCopy;
    }

    public void Clear() => _pendingId = null;

    public (Guid? SimulationId, bool AsCopy) Consume()
    {
        var request = (_pendingId, _asCopy);
        _pendingId = null;
        _asCopy = false;
        return request;
    }
}
