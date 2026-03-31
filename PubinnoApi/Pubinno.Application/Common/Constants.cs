using System.Collections.Generic;

namespace Pubinno.Application.Common;

public static class Constants
{
    public static readonly HashSet<string> ProductIds = new()
    {
        "guinness", "ipa", "lager", "pilsner", "stout", 
        "efes-pilsen", "efes-malt", "bomonti-filtresiz", 
        "tuborg-gold", "tuborg-amber"
    };

    public static readonly HashSet<string> LocationIds = new()
    {
        "istanbul-kadikoy-01", "istanbul-besiktas-01", 
        "izmir-alsancak-01", "ankara-cankaya-01", "london-soho-01"
    };

    public static readonly HashSet<int> Volumes = new()
    {
        200, 250, 284, 330, 355, 400, 473, 500, 568, 1000
    };
}
