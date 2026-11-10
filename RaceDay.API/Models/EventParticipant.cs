using System;
using System.Collections.Generic;

namespace RaceDay.API.Models;

public partial class EventParticipant
{
    public int ParticipantId { get; set; }

    public int EventId { get; set; }

    public string? Field { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Participant Participant { get; set; } = null!;
}
