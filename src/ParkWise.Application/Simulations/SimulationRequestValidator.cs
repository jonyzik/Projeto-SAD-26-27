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

        errors.AddRange(ValidateNeeds(request.Needs));

        var distinctParks = request.CandidateParkIds.Distinct().Count();
        if (distinctParks < MinimumCandidateParks)
            errors.Add($"Selecione pelo menos {MinimumCandidateParks} parques para comparar.");

        var knownCriteria = EvaluationCriteria.Required.Select(criterion => criterion.Kind).ToHashSet();
        if (request.CriterionImportance.Keys.Any(kind => !knownCriteria.Contains(kind)))
            errors.Add("Foi indicada a importância de um critério desconhecido.");
        if (request.CriterionImportance.Values.Any(value => !ImportanceScale.IsValid(value)))
            errors.Add($"A importância de cada critério tem de estar entre {ImportanceScale.Minimum} e {ImportanceScale.Maximum}.");

        if (request.MandatoryCriteria.Any(kind => !knownCriteria.Contains(kind)))
            errors.Add("Foi indicado um critério obrigatório desconhecido.");

        return errors;
    }

    public static IReadOnlyList<string> ValidateNeeds(CompanyNeeds needs)
    {
        ArgumentNullException.ThrowIfNull(needs);
        var errors = new List<string>();
        if (needs.RequiredAreaSquareMetres <= 0)
            errors.Add("A área necessária tem de ser superior a 0 m².");
        if (needs.ContractDurationMonths is < 1 or > MaximumContractMonths)
            errors.Add($"A duração do contrato tem de estar entre 1 e {MaximumContractMonths} meses.");
        return errors;
    }
}
