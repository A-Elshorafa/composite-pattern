namespace ApartmentComposite.Domain;

/// <summary>
/// Composite base: holds child components and computes every total by delegating to them.
/// It never checks whether a child is a Room or another composite.
/// </summary>
public abstract class PropertyComposite(int id, string name) : IPropertyComponent
{
    private readonly List<IPropertyComponent> _children = [];

    public int Id { get; } = id;
    public string Name { get; } = name;
    public IReadOnlyList<IPropertyComponent> Children => _children;

    public int TotalRooms => _children.Sum(c => c.TotalRooms);
    public int AvailableRooms => _children.Sum(c => c.AvailableRooms);
    public decimal PotentialMonthlyIncome => _children.Sum(c => c.PotentialMonthlyIncome);
    public decimal CurrentMonthlyIncome => _children.Sum(c => c.CurrentMonthlyIncome);

    protected void AddChild(IPropertyComponent child) => _children.Add(child);
}
