using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PubinnoApi.Data;
using System;
using System.Linq;

namespace PubinnoApi.Features.Taps;

public static class GetSummaryEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/taps/{deviceId}/summary", async (string deviceId, DateTime from, DateTime to, AppDbContext db) =>
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

            var summary = new GetSummaryResponse
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
    }
}
