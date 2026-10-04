using ClaimsPlatform.Api.Infrastructure.Persistence;
using ClaimsPlatform.Api.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("ClaimsDatabase") 
    ?? throw new InvalidOperationException(
        "Connection string 'ClaimsDatabase' was not found");

builder.Services.AddDbContext<ClaimsDbContext>(options => 
    options.UseNpgsql(connectionString)); 


// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    await using var scope = app.Services.CreateAsyncScope();

    var dbContext =
        scope.ServiceProvider.GetRequiredService<ClaimsDbContext>();

    await dbContext.Database.MigrateAsync();
    await DemoDataSeeder.SeedAsync(dbContext);
}

app.UseHttpsRedirection();

app.Run();
