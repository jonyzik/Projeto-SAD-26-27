using ParkWise.Core;

namespace ParkWise.Application.Simulations;

public static class SimulationRequestValidator
{
    public const int MinimumCandidateParks = 2;
    public const int MaximumContractMonths = 240;

    public static IReadOnlyList<string> Validate(SimulationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Indique um nome para a simulação.");
        else if (request.Name.Trim().Length > 200)
            errors.Add("O nome da simulação não pode ter mais de 200 caracteres.");

        if (request.Description?.Length > 1000)
            errors.Add("A descrição não pode ter mais de 1000 caracteres.");

        errors.AddRange(ValidateParameters(request.Parameters));

        var distinctParks = request.CandidateParkIds.Distinct().Count();
        if (distinctParks < MinimumCandidateParks)
            errors.Add($"Selecione pelo menos {MinimumCandidateParks} parques para comparar.");

        var knownCriteria = EvaluationCriteria.Required.Select(criterion => criterion.Kind).ToHashSet();
        foreach (var comparison in request.Comparisons)
        {
            if (!knownCriteria.Contains(comparison.Left) || !knownCriteria.Contains(comparison.Right) || comparison.Left == comparison.Right)
            {
                errors.Add("Existe uma comparação entre critérios inválida.");
                break;
            }
            if (!SaatyScale.IsValid(comparison.Value))
            {
                errors.Add("As comparações entre critérios têm de usar a escala de Saaty (1/9 a 9).");
                break;
            }
        }

        if (request.MandatoryCriteria.Any(kind => !knownCriteria.Contains(kind)))
            errors.Add("Foi indicado um critério obrigatório desconhecido.");

        return errors;
    }

    public static IReadOnlyList<string> ValidateParameters(SimulationParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        var errors = new List<string>();
        var needs = parameters.Needs;
        var costs = parameters.Costs;

        if (needs.RequiredAreaSquareMetres <= 0)
            errors.Add("A área necessária tem de ser superior a 0 m².");
        if (needs.EmployeeCount < 1)
            errors.Add("O número de colaboradores tem de ser pelo menos 1.");
        if (needs.ContractDurationMonths is < 1 or > MaximumContractMonths)
            errors.Add($"A duração do contrato tem de estar entre 1 e {MaximumContractMonths} meses.");
        if (needs.MaxMonthlyBudget is <= 0)
            errors.Add("O orçamento mensal máximo, quando indicado, tem de ser superior a 0 €.");

        if (costs.ServiceChargePerSquareMetre < 0 || costs.UtilitiesPerSquareMetre < 0 ||
            costs.ServicesPerEmployee < 0 || costs.ParkingCostPerSpace < 0 || costs.SetupCostPerSquareMetre < 0)
            errors.Add("Os parâmetros de custo não podem ser negativos.");
        if (costs.ParkingSpacesRatio is < 0 or > 1)
            errors.Add("A proporção de lugares de estacionamento tem de estar entre 0 e 1.");
        if (costs.ContingencyPercentage is < 0 or > 100)
            errors.Add("A margem de contingência tem de estar entre 0 % e 100 %.");

        return errors;
    }
}
