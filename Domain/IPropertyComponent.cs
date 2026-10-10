namespace ApartmentComposite.Domain;

/// <summary>
/// Component: the common interface shared by leaves (Room) and composites (Apartment, Building).
/// Clients can ask any node for its totals without knowing what kind of node it is.
/// </summary>
public interface IPropertyComponent
{
    int Id { get; }
    string Name { get; }
    int TotalRooms { get; }
    int AvailableRooms { get; }
    decimal PotentialMonthlyIncome { get; }
    decimal CurrentMonthlyIncome { get; }
}
