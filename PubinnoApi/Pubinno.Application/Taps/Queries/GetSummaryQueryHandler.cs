using MediatR;
using Microsoft.EntityFrameworkCore;
using Pubinno.Application.Taps.Dtos;
using Pubinno.Data.Contexts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pubinno.Application.Taps.Queries;

public class GetSummaryQueryHandler : IRequestHandler<GetSummaryQuery, GetSummaryResponseDto>
{
    private readonly AppDbContext _db;

    public GetSummaryQueryHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<GetSummaryResponseDto> Handle(GetSummaryQuery request, CancellationToken cancellationToken)
    {
        var pours = await _db.Pours
            .AsNoTracking()
            .Where(p => p.DeviceId == request.DeviceId && p.StartedAt >= request.From && p.StartedAt < request.To)
            .ToListAsync(cancellationToken);

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

        return new GetSummaryResponseDto
        {
            DeviceId = request.DeviceId,
            From = request.From,
            To = request.To,
            TotalVolumeMl = totalVolume,
            TotalPours = totalPours,
            TopProduct = byProduct.FirstOrDefault(),
            TopLocation = byLocation.FirstOrDefault(),
            ByProduct = byProduct,
            ByLocation = byLocation
        };
    }
}
