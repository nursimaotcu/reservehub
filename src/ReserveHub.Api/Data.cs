using Microsoft.EntityFrameworkCore;
namespace ReserveHub.Api;
public sealed class User {
 public Guid Id {get;set;}=Guid.NewGuid();
 public string Email {get;set;}="";
 public string PasswordHash {get;set;}="";
 public string Role {get;set;}="Member";
}
public sealed class Room {
 public Guid Id {get;set;}=Guid.NewGuid();
 public string Name {get;set;}="";
 public int Capacity {get;set;}
}
public sealed class Booking {
 public Guid Id {get;set;}=Guid.NewGuid();
 public Guid UserId {get;set;}
 public Guid RoomId {get;set;}
 public DateTimeOffset Start {get;set;}
 public DateTimeOffset End {get;set;}
 public bool Cancelled {get;set;}
 public DateTimeOffset CreatedAt {get;set;}=DateTimeOffset.UtcNow;
}
public sealed class AppDb(DbContextOptions<AppDb> options):DbContext(options) {
 public DbSet<User> Users=>Set<User>();
 public DbSet<Room> Rooms=>Set<Room>();
 public DbSet<Booking> Bookings=>Set<Booking>();
 protected override void OnModelCreating(ModelBuilder b) {
  b.Entity<User>().HasIndex(x=>x.Email).IsUnique();
  b.Entity<User>().Property(x=>x.Email).HasMaxLength(254);
  b.Entity<User>().Property(x=>x.Role).HasMaxLength(20);
  b.Entity<Room>().Property(x=>x.Name).HasMaxLength(100);
  b.Entity<Room>().ToTable(t=>t.HasCheckConstraint("room_capacity","\"Capacity\" BETWEEN 1 AND 100"));
  b.Entity<Booking>().HasOne<User>().WithMany().HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Booking>().HasOne<Room>().WithMany().HasForeignKey(x=>x.RoomId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Booking>().HasIndex(x=>new {x.UserId,x.CreatedAt});
  b.Entity<Booking>().ToTable(t=>t.HasCheckConstraint("booking_duration","\"End\" > \"Start\""));
 }
}
