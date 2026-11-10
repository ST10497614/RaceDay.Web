using System;
using System.Collections.Generic;

namespace RaceDay.API.Models;

public partial class RouteInformation
{
    public int RouteId { get; set; }

    public int EventId { get; set; }

    public string RouteName { get; set; } = null!;

    public string StartPoint { get; set; } = null!;

    public string EndPoint { get; set; } = null!;

    public virtual Event Event { get; set; } = null!;
}
