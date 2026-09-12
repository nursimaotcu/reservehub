using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
namespace ReserveHub.Api;
public static class AuthEndpoints {
 public static void MapAuth(this WebApplication app) {
  var group=app.MapGroup("/api/auth").WithTags("Authentication").AddEndpointFilter<ValidationFilter>().RequireRateLimiting("auth");
  group.MapPost("/register",async (Credentials input,AppDb db,IPasswordHasher<User> hasher,CancellationToken ct)=> {
   var user=new User{Email=input.Email.Trim().ToLowerInvariant()};
   user.PasswordHash=hasher.HashPassword(user,input.Password);db.Users.Add(user);
   try {await db.SaveChangesAsync(ct);} catch(DbUpdateException e) when(e.InnerException is PostgresException{SqlState:"23505"}) {return Results.Conflict(new {message="Email already registered."});}
   return Results.Created("/api/auth/me",new {user.Id,user.Email,user.Role});
  });
  group.MapPost("/login",async (Credentials input,AppDb db,IPasswordHasher<User> hasher,IConfiguration config,CancellationToken ct)=> {
   var user=await db.Users.SingleOrDefaultAsync(x=>x.Email==input.Email.Trim().ToLowerInvariant(),ct);
   if(user is null || hasher.VerifyHashedPassword(user,user.PasswordHash,input.Password)==PasswordVerificationResult.Failed) return Results.Unauthorized();
   var expires=DateTime.UtcNow.AddMinutes(15);
   var token=new JwtSecurityToken("ReserveHub","ReserveHub",[new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),new Claim(ClaimTypes.Role,user.Role)],expires:expires,signingCredentials:new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),SecurityAlgorithms.HmacSha256));
   return Results.Ok(new {accessToken=new JwtSecurityTokenHandler().WriteToken(token),expiresAt=expires});
  });
  app.MapGet("/api/auth/me",async (ClaimsPrincipal principal,AppDb db,CancellationToken ct)=> {
   var id=Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
   return Results.Ok(await db.Users.Where(x=>x.Id==id).Select(x=>new {x.Id,x.Email,x.Role}).SingleAsync(ct));
  }).RequireAuthorization().WithTags("Authentication");
 }
}
