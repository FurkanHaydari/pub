using MediatR;
using Pubinno.Application.Taps.Dtos;
using System;

namespace Pubinno.Application.Taps.Queries;

public class GetSummaryQuery : IRequest<GetSummaryResponseDto>
{
    public string DeviceId { get; set; } = string.Empty;
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public GetSummaryQuery(string deviceId, DateTime from, DateTime to)
    {
        DeviceId = deviceId;
        From = from;
        To = to;
    }
}
