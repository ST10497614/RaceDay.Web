
using System;

namespace RaceDay.API.Models;

public partial class Result
{
    public int ResultId { get; set; }

    public int EnrolmentId { get; set; }

    public TimeOnly? FinishTime { get; set; }

    public int? Position { get; set; }

    public virtual Enrolment Enrolment { get; set; } = null!;
}
