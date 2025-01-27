using System.Text;
using API.Data;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using API.Middleware;
using API.Services;
using API.SingalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
   .WithOrigins("Http://localhost:4200","Https://localhost:4200"));

// order is important.
app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();


app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/message");
app.MapFallbackToController("Index", "Fallback");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
   var context = services.GetRequiredService<DataContext>();
   var userManager = services.GetRequiredService<UserManager<AppUser>>();
   var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
   await context.Database.MigrateAsync();
   await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]");
   await context.Database.ExecuteSqlRawAsync("DELETE FROM \"Connections\"");
   await Seed.SeedUsers(userManager, roleManager);
}
catch (Exception ex)
{
   var logger = services.GetRequiredService<ILogger<Program>>();
   logger.LogError(ex, "An error occurred during migration");
}

app.Run();
