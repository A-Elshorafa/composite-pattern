using ApartmentComposite.Domain;

namespace ApartmentComposite.Services;

/// <summary>In-memory store for the building trees. Also seeds a sample building.</summary>
public class PropertyService
{
    private readonly object _lock = new();
    private readonly List<Building> _buildings = [];
    private readonly Dictionary<int, Apartment> _apartments = [];
    private readonly Dictionary<int, Room> _rooms = [];
    private readonly Dictionary<int, int> _roomToApartment = [];
    private readonly Dictionary<int, int> _apartmentToBuilding = [];
    private int _nextId = 1;

    public PropertyService() => Seed();

    public IReadOnlyList<Building> Buildings { get { lock (_lock) return [.. _buildings]; } }

    public Building? FindBuilding(int id) { lock (_lock) return _buildings.FirstOrDefault(b => b.Id == id); }

    public Building CreateBuilding(string name, string address)
    {
        lock (_lock)
        {
            var b = new Building(_nextId++, name, address);
            _buildings.Add(b);
            return b;
        }
    }

    public Apartment AddApartment(int buildingId, string name)
    {
        lock (_lock)
        {
            var building = FindBuilding(buildingId) ?? throw new KeyNotFoundException($"Building {buildingId} not found.");
            var apt = new Apartment(_nextId++, name);
            building.AddApartment(apt);
            _apartments[apt.Id] = apt;
            _apartmentToBuilding[apt.Id] = buildingId;
            return apt;
        }
    }

    public Room AddRoom(int apartmentId, string name, decimal monthlyPrice)
    {
        lock (_lock)
        {
            var apt = _apartments.GetValueOrDefault(apartmentId) ?? throw new KeyNotFoundException($"Apartment {apartmentId} not found.");
            var room = new Room(_nextId++, name, monthlyPrice);
            apt.AddRoom(room);
            _rooms[room.Id] = room;
            _roomToApartment[room.Id] = apartmentId;
            return room;
        }
    }

    public Apartment? FindApartment(int id) { lock (_lock) return _apartments.GetValueOrDefault(id); }

    public Room RentRoom(int roomId, string tenant)
    {
        lock (_lock)
        {
            var room = _rooms.GetValueOrDefault(roomId) ?? throw new KeyNotFoundException($"Room {roomId} not found.");
            room.Rent(tenant);
            return room;
        }
    }

    public Room ReleaseRoom(int roomId)
    {
        lock (_lock)
        {
            var room = _rooms.GetValueOrDefault(roomId) ?? throw new KeyNotFoundException($"Room {roomId} not found.");
            room.Release();
            return room;
        }
    }

    public IReadOnlyList<(Room Room, Apartment Apartment, Building Building)> AvailableRooms()
    {
        lock (_lock)
            return [.. _rooms.Values
                .Where(r => !r.IsRented)
                .OrderBy(r => r.Id)
                .Select(r =>
                {
                    var apt = _apartments[_roomToApartment[r.Id]];
                    return (r, apt, FindBuilding(_apartmentToBuilding[apt.Id])!);
                })];
    }

    private void Seed()
    {
        var tower = CreateBuilding("Nile Tower", "12 Corniche St, Cairo");
        var a1 = AddApartment(tower.Id, "Apartment 1A");
        AddRoom(a1.Id, "Master bedroom", 400);
        AddRoom(a1.Id, "Small bedroom", 250);
        var a2 = AddApartment(tower.Id, "Apartment 2B");
        AddRoom(a2.Id, "Room 1", 300);
        AddRoom(a2.Id, "Room 2", 300);
        AddRoom(a2.Id, "Room 3", 280);
        RentRoom(a1.Children[0].Id, "Omar");
    }
}
