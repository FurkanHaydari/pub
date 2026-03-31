using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PubinnoApi;
using PubinnoApi.Data;
using PubinnoApi.Dtos;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Render and Neon fallback
    connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

var appBuilder = builder.Build();

using (var scope = appBuilder.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// X-API-Key Middleware
appBuilder.Use(async (context, next) =>
{
    // Health check endpoint skips authentication
    if (context.Request.Path == "/health")
    {
        await next(context);
        return;
    }

    if (!context.Request.Headers.TryGetValue("X-API-Key", out var extractedApiKey))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("API Key existsnt.");
        return;
    }

    var apiKey = appBuilder.Configuration.GetValue<string>("ApiKeyConfig:ApiKey") 
        ?? Environment.GetEnvironmentVariable("API_KEY");
        
    if (!apiKey!.Equals(extractedApiKey))
    {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Unauthorized.");
        return;
    }

    await next(context);
});

appBuilder.MapGet("/health", async (AppDbContext db) =>
{
    try 
    {
        var canConnect = await db.Database.CanConnectAsync();
        if (canConnect) 
            return Results.Ok(new { status = "ok", db = "ok" });
        return Results.StatusCode(500);
    }
    catch 
    {
        return Results.StatusCode(500);
    }
});

appBuilder.MapPost("/v1/pours", async (PourRequest request, AppDbContext db) =>
{
    if (request.EventId == Guid.Empty)
        return Results.BadRequest("Invalid EventId.");

    if (!Constants.ProductIds.Contains(request.ProductId))
        return Results.BadRequest("Invalid ProductId.");

    if (!Constants.LocationIds.Contains(request.LocationId))
        return Results.BadRequest("Invalid LocationId.");

    if (!Constants.Volumes.Contains(request.VolumeMl))
        return Results.BadRequest("Invalid VolumeMl.");

    if (request.EndedAt < request.StartedAt)
        return Results.BadRequest("EndedAt cannot be earlier than StartedAt.");
        
    var pour = new Pour
    {
        EventId = request.EventId,
        DeviceId = request.DeviceId,
        LocationId = request.LocationId,
        ProductId = request.ProductId,
        StartedAt = request.StartedAt,
        EndedAt = request.EndedAt,
        VolumeMl = request.VolumeMl
    };

    db.Pours.Add(pour);

    try
    {
        await db.SaveChangesAsync();
        return Results.Ok();
    }
    catch (DbUpdateException ex)
    {
        if (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
        {
            // Idempotent retry, just return 200 OK without adding to DB
            return Results.Ok();
        }
        throw;
    }
});

appBuilder.MapGet("/v1/taps/{deviceId}/summary", async (string deviceId, DateTime from, DateTime to, AppDbContext db) =>
{
    var pours = await db.Pours
        .AsNoTracking()
        .Where(p => p.DeviceId == deviceId && p.StartedAt >= from && p.StartedAt < to)
        .ToListAsync();

    var totalVolume = pours.Sum(p => p.VolumeMl);
    var totalPours = pours.Count;

    var byProduct = pours
        .GroupBy(p => p.ProductId)
        .Select(g => new ProductStat
        {
            ProductId = g.Key,
            VolumeMl = g.Sum(x => x.VolumeMl),
            Pours = g.Count()
        })
        .OrderByDescending(s => s.VolumeMl)
        .ToList();

    var byLocation = pours
        .GroupBy(p => p.LocationId)
        .Select(g => new LocationStat
        {
            LocationId = g.Key,
            VolumeMl = g.Sum(x => x.VolumeMl),
            Pours = g.Count()
        })
        .OrderByDescending(s => s.VolumeMl)
        .ToList();

    var summary = new SummaryResponse
    {
        DeviceId = deviceId,
        From = from,
        To = to,
        TotalVolumeMl = totalVolume,
        TotalPours = totalPours,
        TopProduct = byProduct.FirstOrDefault(),
        TopLocation = byLocation.FirstOrDefault(),
        ByProduct = byProduct,
        ByLocation = byLocation
    };

    return Results.Ok(summary);
});

appBuilder.Run();
