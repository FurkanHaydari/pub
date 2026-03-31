using System;
using System.Collections.Generic;

namespace PubinnoApi.Dtos;

public class SummaryResponse
{
    public string DeviceId { get; set; } = string.Empty;
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int TotalVolumeMl { get; set; }
    public int TotalPours { get; set; }
    public ProductStat? TopProduct { get; set; }
    public LocationStat? TopLocation { get; set; }
    public List<ProductStat> ByProduct { get; set; } = new();
    public List<LocationStat> ByLocation { get; set; } = new();
}

public class ProductStat
{
    public string ProductId { get; set; } = string.Empty;
    public int VolumeMl { get; set; }
    public int Pours { get; set; }
}

public class LocationStat
{
    public string LocationId { get; set; } = string.Empty;
    public int VolumeMl { get; set; }
    public int Pours { get; set; }
}
