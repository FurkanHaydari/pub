using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pubinno.Application;
using Pubinno.Data;
using Pubinno.Data.Contexts;
using System;

var builder = WebApplication.CreateBuilder(args);

// Controllers & FluentValidation
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Custom 400 responses can be configured here if necessary
    });

// FluentValidation automatic integration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

// Data and Application Registrations
builder.Services.AddDataServices(builder.Configuration);
builder.Services.AddApplicationServices();

// Dynamic PORT resolution for Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

var appBuilder = builder.Build();

// Ensure Database Created automatically (Entity Framework Core Seeding)
using (var scope = appBuilder.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// Global X-API-Key Middleware
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

// Map Controllers
appBuilder.MapControllers();

appBuilder.Run();
