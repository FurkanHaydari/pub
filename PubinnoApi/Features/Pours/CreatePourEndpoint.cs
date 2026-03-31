using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using PubinnoApi.Data;
using PubinnoApi.Features.Common;
using System;
using System.Threading.Tasks;

namespace PubinnoApi.Features.Pours;

public static class CreatePourEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/pours", async (CreatePourRequest request, AppDbContext db) =>
        {
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
                    return Results.Ok();
                }
                throw;
            }
        })
        .AddEndpointFilter<ValidationFilter<CreatePourRequest>>();
    }
}
