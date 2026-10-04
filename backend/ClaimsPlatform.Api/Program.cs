using System.Text.Json.Serialization;
using ClaimsPlatform.Api.Access.Authentication;
using ClaimsPlatform.Api.Access.Authorization;
using ClaimsPlatform.Api.Access.Domain;
using ClaimsPlatform.Api.Access.Endpoints;
using ClaimsPlatform.Api.Claims.Endpoints;
using ClaimsPlatform.Api.Common.Errors;
using ClaimsPlatform.Api.Infrastructure.Persistence;
using ClaimsPlatform.Api.Infrastructure.Seeding;
using ClaimsPlatform.Api.WorkManagement.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("ClaimsDatabase") 
    ?? throw new InvalidOperationException(
        "Connection string 'ClaimsDatabase' was not found");

builder.Services.AddDbContext<ClaimsDbContext>(options => 
    options.UseNpgsql(connectionString)); 

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services
    .AddAuthentication(DemoAuthenticationDefaults.Scheme)
    .AddScheme<AuthenticationSchemeOptions, DemoAuthenticationHandler>(
        DemoAuthenticationDefaults.Scheme,
        _ => { });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        AccessPolicies.Claimant,
        policy => policy.RequireRole(UserRole.Claimant.ToString()));

    options.AddPolicy(
        AccessPolicies.ClaimsOfficer,
        policy => policy.RequireRole(UserRole.ClaimsOfficer.ToString()));

    options.AddPolicy(
        AccessPolicies.Manager,
        policy => policy.RequireRole(UserRole.Manager.ToString()));
});


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

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapAccessEndpoints();
app.MapClaimantClaimEndpoints();
app.MapOfficerWorkEndpoints();

app.Run();
