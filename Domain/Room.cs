namespace ApartmentComposite.Domain;

/// <summary>Leaf: a rentable room. It has no children and computes its totals by itself.</summary>
public class Room(int id, string name, decimal monthlyPrice) : IPropertyComponent
{
    public int Id { get; } = id;
    public string Name { get; } = name;
    public decimal MonthlyPrice { get; } = monthlyPrice;
    public string? Tenant { get; private set; }
    public bool IsRented => Tenant is not null;

    public int TotalRooms => 1;
    public int AvailableRooms => IsRented ? 0 : 1;
    public decimal PotentialMonthlyIncome => MonthlyPrice;
    public decimal CurrentMonthlyIncome => IsRented ? MonthlyPrice : 0;

    public void Rent(string tenant)
    {
        if (IsRented) throw new InvalidOperationException($"Room '{Name}' is already rented by {Tenant}.");
        Tenant = tenant;
    }

    public void Release()
    {
        if (!IsRented) throw new InvalidOperationException($"Room '{Name}' is not rented.");
        Tenant = null;
    }
}
