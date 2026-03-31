using System;

namespace Pubinno.Data.Entities;

public class Pour
{
    public Guid EventId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string LocationId { get; set; } = string.Empty;
    public Location Location { get; set; } = null!;
    public string ProductId { get; set; } = string.Empty;
    public Product Product { get; set; } = null!;
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public int VolumeMl { get; set; }
}
