using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PubinnoApi;
using PubinnoApi.Data;
using PubinnoApi.Features.Pours;
using PubinnoApi.Features.Taps;
using System;

var builder = WebApplication.CreateBuilder(args);

// DbContext configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? Environment.GetEnvironmentVariable("DATABASE_URL");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register FluentValidation Validators
builder.Services.AddValidatorsFromAssemblyContaining<CreatePourValidator>();

// Dynamic PORT resolution for Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

var appBuilder = builder.Build();

// Ensure Database Created automatically
using (var scope = appBuilder.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// X-API-Key Middleware (Global Authentication Filter)
appBuilder.Use(async (context, next) =>
{
    if (context.Request.Path == "/health")
    {
        await next(context);
        return;
    }

    if (!context.Request.Headers.TryGetValue("X-API-Key", out var extractedApiKey))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("API Key missing.");
        return;
    }

    var apiKey = appBuilder.Configuration.GetValue<string>("ApiKeyConfig:ApiKey") 
        ?? Environment.GetEnvironmentVariable("API_KEY");
        
    if (!apiKey!.Equals(extractedApiKey))
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.WriteAsync("Unauthorized.");
        return;
    }

    await next(context);
});

// Centralized Health Mapping
appBuilder.MapGet("/health", async (AppDbContext db) =>
{
    try 
    {
        var canConnect = await db.Database.CanConnectAsync();
        return canConnect ? Results.Ok(new { status = "ok", db = "ok" }) : Results.StatusCode(500);
    }
    catch 
    {
        return Results.StatusCode(500);
    }
});

// Vertical Slice Endpoints Registration
CreatePourEndpoint.Map(appBuilder);
GetSummaryEndpoint.Map(appBuilder);

appBuilder.Run();
