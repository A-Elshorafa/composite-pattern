using ApartmentComposite.Domain;
using ApartmentComposite.Models;
using ApartmentComposite.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PropertyService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(o => o.RoutePrefix = "swagger");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// Recursively maps any node to a DTO. The totals come straight from the component interface,
// so the same code serves a Room, an Apartment and a Building.
static NodeResponse ToNode(IPropertyComponent c) => c switch
{
    Room r => new(r.Id, "Room", r.Name, r.TotalRooms, r.AvailableRooms, r.PotentialMonthlyIncome,
        r.CurrentMonthlyIncome, r.MonthlyPrice, r.Tenant, null),
    PropertyComposite p => new(p.Id, p.GetType().Name, p.Name, p.TotalRooms, p.AvailableRooms,
        p.PotentialMonthlyIncome, p.CurrentMonthlyIncome, null, null, [.. p.Children.Select(ToNode)]),
    _ => throw new NotSupportedException()
};

static bool Invalid(object body, out IResult? error)
{
    var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
    var ok = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
        body, new(body), results, validateAllProperties: true);
    error = ok ? null : Results.BadRequest(string.Join("; ", results.Select(r => r.ErrorMessage)));
    return !ok;
}

app.MapPost("/api/buildings", (CreateBuildingRequest req, PropertyService svc) =>
{
    if (Invalid(req, out var err)) return err!;
    var b = svc.CreateBuilding(req.Name, req.Address);
    return Results.Created($"/api/buildings/{b.Id}", ToNode(b));
})
.WithSummary("Create an empty building (composite)");

app.MapGet("/api/buildings", (PropertyService svc) =>
    svc.Buildings.Select(b => new BuildingSummary(b.Id, b.Name, b.Address, b.TotalRooms, b.AvailableRooms, b.CurrentMonthlyIncome)))
.WithSummary("List buildings with aggregated totals");

app.MapGet("/api/buildings/{id:int}", (int id, PropertyService svc) =>
    svc.FindBuilding(id) is { } b ? Results.Ok(ToNode(b)) : Results.NotFound($"Building {id} not found."))
.WithSummary("Get the whole tree: building → apartments → rooms, with totals at every level");

app.MapPost("/api/buildings/{buildingId:int}/apartments", (int buildingId, CreateApartmentRequest req, PropertyService svc) =>
{
    if (Invalid(req, out var err)) return err!;
    try { return Results.Created($"/api/buildings/{buildingId}", ToNode(svc.AddApartment(buildingId, req.Name))); }
    catch (KeyNotFoundException ex) { return Results.NotFound(ex.Message); }
})
.WithSummary("Add an apartment (composite) to a building");

app.MapPost("/api/apartments/{apartmentId:int}/rooms", (int apartmentId, CreateRoomRequest req, PropertyService svc) =>
{
    if (Invalid(req, out var err)) return err!;
    try { return Results.Created($"/api/apartments/{apartmentId}", ToNode(svc.AddRoom(apartmentId, req.Name, req.MonthlyPrice))); }
    catch (KeyNotFoundException ex) { return Results.NotFound(ex.Message); }
})
.WithSummary("Add a room (leaf) to an apartment");

app.MapGet("/api/apartments/{id:int}", (int id, PropertyService svc) =>
    svc.FindApartment(id) is { } a ? Results.Ok(ToNode(a)) : Results.NotFound($"Apartment {id} not found."))
.WithSummary("Get an apartment with its rooms and totals");

app.MapGet("/api/rooms/available", (PropertyService svc) =>
    svc.AvailableRooms().Select(x => new AvailableRoomResponse(x.Room.Id, x.Room.Name, x.Room.MonthlyPrice, x.Apartment.Name, x.Building.Name)))
.WithSummary("List every room that can still be rented, across all buildings");

app.MapPost("/api/rooms/{roomId:int}/rent", (int roomId, RentRoomRequest req, PropertyService svc) =>
{
    if (Invalid(req, out var err)) return err!;
    try { return Results.Ok(ToNode(svc.RentRoom(roomId, req.Tenant))); }
    catch (KeyNotFoundException ex) { return Results.NotFound(ex.Message); }
    catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
})
.WithSummary("Rent a room. Apartment and building totals update automatically");

app.MapPost("/api/rooms/{roomId:int}/release", (int roomId, PropertyService svc) =>
{
    try { return Results.Ok(ToNode(svc.ReleaseRoom(roomId))); }
    catch (KeyNotFoundException ex) { return Results.NotFound(ex.Message); }
    catch (InvalidOperationException ex) { return Results.Conflict(ex.Message); }
})
.WithSummary("Release a rented room");

app.Run();
