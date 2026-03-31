using MediatR;
using Microsoft.EntityFrameworkCore;
using Pubinno.Data.Contexts;
using Pubinno.Data.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Pubinno.Application.Pours.Commands;

public class CreatePourCommandHandler : IRequestHandler<CreatePourCommand, bool>
{
    private readonly AppDbContext _db;

    public CreatePourCommandHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> Handle(CreatePourCommand request, CancellationToken cancellationToken)
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

        _db.Pours.Add(pour);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
            {
                // Unique constraint violation (Idempotency) -> Treat as success
                return true;
            }
            throw;
        }
    }
}
