using ClaimsPlatform.Api.Infrastructure.Persistence;
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
}

app.UseHttpsRedirection();

app.Run();
