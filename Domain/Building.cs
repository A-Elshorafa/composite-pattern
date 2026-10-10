namespace ApartmentComposite.Domain;

/// <summary>Composite: a building is made of apartments (which are themselves composites).</summary>
public class Building(int id, string name, string address) : PropertyComposite(id, name)
{
    public string Address { get; } = address;

    public void AddApartment(Apartment apartment) => AddChild(apartment);
}
