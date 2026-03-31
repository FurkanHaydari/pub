using MediatR;
using System;

namespace Pubinno.Application.Pours.Commands;

public class CreatePourCommand : IRequest<bool>
{
    public Guid EventId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string LocationId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public int VolumeMl { get; set; }
}
