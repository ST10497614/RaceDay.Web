using System;
using System.Collections.Generic;

namespace RaceDay.API.Models;

public partial class Event
{
    public int EventId { get; set; }

    public int OrganiserId { get; set; }
    public int Organiserid { get; internal set; }
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime EventDate { get; set; }

    public string Location { get; set; } = null!;

    public int? CategoryId { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<EventParticipant> EventParticipants { get; set; } = new List<EventParticipant>();

    public virtual Organiser Organiser { get; set; } = null!;

    public virtual ICollection<RouteInformation> RouteInformations { get; set; } = new List<RouteInformation>();

    public virtual ICollection<Category> CategoriesNavigation { get; set; } = new List<Category>();
}
