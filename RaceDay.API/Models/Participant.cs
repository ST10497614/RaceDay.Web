using System;
using System.Collections.Generic;

namespace RaceDay.API.Models;

public class Participant
{
    public int ParticipantID { get; set; }

    public string ParticipantName { get; set; } = string.Empty;

    public string ParticipantSurname { get; set; } = string.Empty;

    public string? Field { get; set; }

    public string? Email { get; set; }

    public string? PasswordHash { get; set; }
    
    public virtual ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();

    public virtual ICollection<EventParticipant> EventParticipants { get; set; } = new List<EventParticipant>();
}
