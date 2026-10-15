namespace ParkWise.Application.ScienceParks;

public sealed class ScienceParkSelection
{
    private readonly List<Guid> _selectedIds = [];

    public event EventHandler? Changed;

    public IReadOnlyList<Guid> SelectedIds => _selectedIds.ToArray();
    public int Count => _selectedIds.Count;
    public bool Contains(Guid id) => _selectedIds.Contains(id);

    public void SetSelected(Guid id, bool selected)
    {
        if (selected)
        {
            if (_selectedIds.Contains(id)) return;
            _selectedIds.Add(id);
        }
        else if (!_selectedIds.Remove(id))
        {
            return;
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }
}
