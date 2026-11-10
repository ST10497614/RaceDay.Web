using System;
using System.Collections.Generic;

namespace RaceDay.API.Models;

public partial class Enrolment
{
    public int EnrolmentId { get; set; }

    public int ParticipantId { get; set; }

    public int CategoryId { get; set; }

    public DateTime? EnrolmentDate { get; set; }

    public string Status { get; set; } = null!;

    public virtual Category Category { get; set; } = null!;

    public virtual Participant Participant { get; set; } = null!;

    public virtual Result? Result { get; set; }
}
