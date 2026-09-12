using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Npgsql;
namespace ReserveHub.Api;
public static class BookingEndpoints {
 public static void MapBookings(this WebApplication app) {
  var rooms=app.MapGroup("/api/rooms").WithTags("Rooms");
  rooms.MapGet("/",async (AppDb db,CancellationToken ct,int page=1,int pageSize=20)=> {
   if(page<1||page>10000||pageSize<1||pageSize>100) return Results.BadRequest(new{message="Invalid pagination."});
   return Results.Ok(await db.Rooms.AsNoTracking().OrderBy(x=>x.Name).ThenBy(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct));
  });
  rooms.MapPost("/",async (RoomInput input,AppDb db,CancellationToken ct)=> {
   if(string.IsNullOrWhiteSpace(input.Name))return Results.BadRequest();
   var room=new Room{Name=input.Name.Trim(),Capacity=input.Capacity};db.Rooms.Add(room);await db.SaveChangesAsync(ct);
   return Results.Created($"/api/rooms/{room.Id}",room);
  }).AddEndpointFilter<ValidationFilter>().RequireAuthorization("Admin");
  rooms.MapGet("/{id:guid}",async (Guid id,AppDb db,CancellationToken ct)=>await db.Rooms.FindAsync([id],ct) is Room room?Results.Ok(room):Results.NotFound());
  var bookings=app.MapGroup("/api/bookings").WithTags("Bookings").RequireAuthorization();
  bookings.MapPost("/",async (BookingInput input,ClaimsPrincipal user,AppDb db,CancellationToken ct)=> {
   var start=input.Start.ToUniversalTime();var end=input.End.ToUniversalTime();
   if(start<=DateTimeOffset.UtcNow||end<=start||end-start>TimeSpan.FromHours(8)||start>DateTimeOffset.UtcNow.AddDays(90))
    return Results.BadRequest(new{message="Use a future interval of at most 8 hours, within 90 days."});
   if(!await db.Rooms.AnyAsync(x=>x.Id==input.RoomId,ct))return Results.NotFound(new{message="Room not found."});
   var booking=new Booking{UserId=Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!),RoomId=input.RoomId,Start=start,End=end};
   db.Bookings.Add(booking);
   // PostgreSQL exclusion constraint also protects simultaneous requests across API instances.
   try {await db.SaveChangesAsync(ct);} catch(DbUpdateException e) when(e.InnerException is PostgresException{SqlState:"23P01"}) {
    return Results.Conflict(new{message="The room is already reserved during this interval."});
   }
   return Results.Created($"/api/bookings/{booking.Id}",booking);
  });
  bookings.MapGet("/",async (ClaimsPrincipal user,AppDb db,CancellationToken ct,int page=1,int pageSize=20)=> {
   if(page<1||page>10000||pageSize<1||pageSize>100)return Results.BadRequest();
   var id=Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
   return Results.Ok(await db.Bookings.AsNoTracking().Where(x=>x.UserId==id).OrderByDescending(x=>x.CreatedAt).ThenBy(x=>x.Id).Skip((page-1)*pageSize).Take(pageSize).ToListAsync(ct));
  });
  bookings.MapGet("/{id:guid}",async(Guid id,ClaimsPrincipal user,AppDb db,CancellationToken ct)=> {
   var uid=Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
   var booking=await db.Bookings.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==id&&x.UserId==uid,ct);
   return booking is null?Results.NotFound():Results.Ok(booking);
  });
  bookings.MapDelete("/{id:guid}",async(Guid id,ClaimsPrincipal user,AppDb db,CancellationToken ct)=> {
   var uid=Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
   // Repeating cancellation is harmless; another user's ID always returns 404.
   var count=await db.Bookings.Where(x=>x.Id==id&&x.UserId==uid).ExecuteUpdateAsync(s=>s.SetProperty(x=>x.Cancelled,true),ct);
   return count==0?Results.NotFound():Results.NoContent();
  });
 }
}
