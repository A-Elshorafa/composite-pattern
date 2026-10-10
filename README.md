# Composite Pattern – Apartment Rental Example (.NET 10)

## What is the Composite pattern?

**Composite** lets you treat a single object and a group of objects through the same interface. A group holds children (which may be single objects or other groups) and answers questions by asking each child.

Here, a **building** contains **apartments**, and an **apartment** contains **rooms**. Only a room can be rented.

## Structure

```
IPropertyComponent            ◄── Component (Domain/IPropertyComponent.cs)
 ├── Room                     ◄── Leaf (rentable, has a price and a tenant)
 └── PropertyComposite        ◄── Composite base (holds children, sums their totals)
      ├── Apartment           (contains Rooms)
      └── Building            (contains Apartments)
```

Every node answers: `TotalRooms`, `AvailableRooms`, `PotentialMonthlyIncome`, `CurrentMonthlyIncome`.

- A `Room` returns its own values (1 room, its price, etc.).
- An `Apartment` or `Building` returns the sum of its children. It never checks the child's type.

Renting a room therefore updates the apartment's and the building's numbers with no extra code.

## Run

```bash
dotnet run
```

Swagger UI: http://localhost:8095/swagger

A sample building ("Nile Tower", id 1) is seeded at startup.

## Endpoints

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/buildings` | Create a building |
| GET | `/api/buildings` | List buildings with totals |
| GET | `/api/buildings/{id}` | Full tree with totals at every level |
| POST | `/api/buildings/{buildingId}/apartments` | Add an apartment |
| POST | `/api/apartments/{apartmentId}/rooms` | Add a room |
| GET | `/api/apartments/{id}` | Apartment with its rooms |
| GET | `/api/rooms/available` | All rooms still free |
| POST | `/api/rooms/{roomId}/rent` | Rent a room (409 if already rented) |
| POST | `/api/rooms/{roomId}/release` | Release a room |

## Try it

1. `GET /api/buildings/1`: building shows 5 rooms, 4 available.
2. `POST /api/rooms/4/rent` with `{ "tenant": "Sara" }`.
3. `GET /api/buildings/1` again: the room, its apartment and the building all show the new totals.
