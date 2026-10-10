namespace ApartmentComposite.Domain;

/// <summary>Composite: an apartment is made of rooms.</summary>
public class Apartment(int id, string name) : PropertyComposite(id, name)
{
    public void AddRoom(Room room) => AddChild(room);
}
