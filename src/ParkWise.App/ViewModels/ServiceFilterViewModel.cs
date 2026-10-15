using ParkWise.Core.Enums;

namespace ParkWise.App.ViewModels;

public sealed class ServiceFilterViewModel(CriterionKind kind, string name, Action changed) : ViewModelBase
{
    private bool _isSelected;

    public CriterionKind Kind { get; } = kind;
    public string Name { get; } = name;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (!SetProperty(ref _isSelected, value)) return;
            changed();
        }
    }
}
