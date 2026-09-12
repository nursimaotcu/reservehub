using System.ComponentModel.DataAnnotations;
namespace ReserveHub.Api;
public sealed record Credentials([property:Required,EmailAddress,MaxLength(254)] string Email,[property:Required,MinLength(12),MaxLength(128)] string Password);
public sealed record RoomInput([property:Required,MinLength(2),MaxLength(100)] string Name,[property:Range(1,100)] int Capacity);
public sealed record BookingInput(Guid RoomId,DateTimeOffset Start,DateTimeOffset End);
public sealed class ValidationFilter:IEndpointFilter {
 public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext ctx,EndpointFilterDelegate next) {
  foreach(var arg in ctx.Arguments.Where(x=>x is Credentials or RoomInput)) {
   var errors=new List<ValidationResult>();
   if(!Validator.TryValidateObject(arg!,new ValidationContext(arg!),errors,true))
    return Results.ValidationProblem(errors.GroupBy(e=>e.MemberNames.FirstOrDefault()??"input").ToDictionary(g=>g.Key,g=>g.Select(e=>e.ErrorMessage!).ToArray()));
  }
  return await next(ctx);
 }
}
