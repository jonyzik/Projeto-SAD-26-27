using System.Text.Json;
using ParkWise.Application.Simulations;

namespace ParkWise.Infrastructure.Simulations;

/// <summary>
/// Guarda os parâmetros predefinidos das simulações num ficheiro JSON local do utilizador
/// (por omissão em %APPDATA%\ParkWise\simulation-parameters.json). Não depende da base de dados.
/// </summary>
public sealed class JsonSimulationParametersStore(string? filePath = null) : ISimulationParametersStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private readonly string _filePath = filePath ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ParkWise", "simulation-parameters.json");

    public async Task<SimulationParameters> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath)) return SimulationParameters.Default;
        try
        {
            await using var stream = File.OpenRead(_filePath);
            var stored = await JsonSerializer.DeserializeAsync<SimulationParameters>(stream, Options, cancellationToken);
            return stored is not null && SimulationRequestValidator.ValidateParameters(stored).Count == 0
                ? stored
                : SimulationParameters.Default;
        }
        catch (Exception exception) when (exception is JsonException or IOException or UnauthorizedAccessException or NotSupportedException)
        {
            return SimulationParameters.Default;
        }
    }

    public async Task SaveAsync(SimulationParameters parameters, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        var temporary = _filePath + ".tmp";
        await using (var stream = File.Create(temporary))
            await JsonSerializer.SerializeAsync(stream, parameters, Options, cancellationToken);
        File.Move(temporary, _filePath, overwrite: true);
    }

    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        if (File.Exists(_filePath)) File.Delete(_filePath);
        return Task.CompletedTask;
    }
}
