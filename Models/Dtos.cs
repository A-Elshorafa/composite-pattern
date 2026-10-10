using System.ComponentModel.DataAnnotations;

namespace ApartmentComposite.Models;

public record CreateBuildingRequest([property: Required] string Name, [property: Required] string Address);
public record CreateApartmentRequest([property: Required] string Name);
public record CreateRoomRequest([property: Required] string Name, [property: Range(0.01, double.MaxValue)] decimal MonthlyPrice);
public record RentRoomRequest([property: Required] string Tenant);

/// <summary>One node of the tree. Rooms have no children and carry price/tenant; composites carry children.</summary>
public record NodeResponse(
    int Id,
    string Type,
    string Name,
    int TotalRooms,
    int AvailableRooms,
    decimal PotentialMonthlyIncome,
    decimal CurrentMonthlyIncome,
    decimal? MonthlyPrice,
    string? Tenant,
    IReadOnlyList<NodeResponse>? Children);

public record BuildingSummary(int Id, string Name, string Address, int TotalRooms, int AvailableRooms, decimal CurrentMonthlyIncome);
public record AvailableRoomResponse(int RoomId, string Name, decimal MonthlyPrice, string Apartment, string Building);
