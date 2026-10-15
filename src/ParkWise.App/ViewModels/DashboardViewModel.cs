namespace ParkWise.App.ViewModels;

public sealed class DashboardViewModel
{
    public string DemoNotice => "Dados fictícios para demonstração da interface. Não representam parques reais nem resultados AHP.";
    public IReadOnlyList<DashboardStat> Stats { get; } =
    [
        new("Parques tecnológicos", "12", "Valor fictício"),
        new("Critérios de avaliação", "9", "Critérios obrigatórios"),
        new("Simulações", "4", "Valor fictício"),
        new("Custo médio mensal", "1 250 €", "Valor fictício")
    ];

    public IReadOnlyList<DemoParkRow> Parks { get; } =
    [
        new("Parque Exemplo A", "Cidade fictícia", "1 100 €", "Fictício"),
        new("Parque Exemplo B", "Cidade fictícia", "1 350 €", "Fictício"),
        new("Parque Exemplo C", "Cidade fictícia", "1 300 €", "Fictício")
    ];
}

public sealed record DashboardStat(string Label, string Value, string Caption);
public sealed record DemoParkRow(string Name, string City, string MonthlyCost, string Status);
