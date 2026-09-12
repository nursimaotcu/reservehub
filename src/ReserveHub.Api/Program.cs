using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ReserveHub.Api;
var builder=WebApplication.CreateBuilder(args);
var key=builder.Configuration["Jwt:Key"];
if(string.IsNullOrWhiteSpace(key)||Encoding.UTF8.GetByteCount(key)<32)throw new InvalidOperationException("Set Jwt__Key to a random secret of at least 32 bytes.");
builder.Logging.ClearProviders();builder.Logging.AddJsonConsole();
builder.Services.AddProblemDetails();builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDb>(o=>o.UseNpgsql(builder.Configuration.GetConnectionString("Database")??throw new InvalidOperationException("Set ConnectionStrings__Database.")));
builder.Services.AddScoped<IPasswordHasher<User>,PasswordHasher<User>>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>o.TokenValidationParameters=new TokenValidationParameters {
 ValidateIssuer=true,ValidIssuer="ReserveHub",ValidateAudience=true,ValidAudience="ReserveHub",ValidateIssuerSigningKey=true,
 IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),ValidateLifetime=true,ClockSkew=TimeSpan.FromSeconds(15)
});
builder.Services.AddAuthorization(o=>o.AddPolicy("Admin",p=>p.RequireRole("Admin")));
builder.Services.AddRateLimiter(o=> {
 o.RejectionStatusCode=429;
 o.AddPolicy("auth",ctx=>RateLimitPartition.GetFixedWindowLimiter(ctx.Connection.RemoteIpAddress?.ToString()??"unknown",_=>new FixedWindowRateLimiterOptions{PermitLimit=20,Window=TimeSpan.FromMinutes(1),QueueLimit=0}));
});
var app=builder.Build();
app.UseExceptionHandler();app.UseStatusCodePages();
app.UseAuthentication();app.UseAuthorization();app.UseRateLimiter();
app.MapOpenApi();
app.MapGet("/health/live",()=>Results.Ok(new{status="live"}));
app.MapGet("/health/ready",async(AppDb db,CancellationToken ct)=>await db.Database.CanConnectAsync(ct)?Results.Ok(new{status="ready"}):Results.StatusCode(503));
app.MapAuth();app.MapBookings();
if(args.Contains("--migrate")) {
 await using var scope=app.Services.CreateAsyncScope();
 var db=scope.ServiceProvider.GetRequiredService<AppDb>();await db.Database.MigrateAsync();
 var email=builder.Configuration["Bootstrap:Email"];var password=builder.Configuration["Bootstrap:Password"];
 if(!string.IsNullOrWhiteSpace(email)&&!string.IsNullOrWhiteSpace(password)) {
  if(password.Length<12)throw new InvalidOperationException("Admin password must have at least 12 characters.");
  email=email.Trim().ToLowerInvariant();
  if(!await db.Users.AnyAsync(x=>x.Email==email)) {
   var admin=new User{Email=email,Role="Admin"};admin.PasswordHash=scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>().HashPassword(admin,password);db.Users.Add(admin);await db.SaveChangesAsync();
  }
 }
 return;
}
app.Run();
public partial class Program {}
