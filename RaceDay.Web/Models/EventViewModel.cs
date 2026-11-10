using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Web.Models;

public class EventViewModel
{
    [JsonPropertyName("eventId")]
    public int EventId { get; set; }

    [Required(ErrorMessage = "Event title is required.")]
    [StringLength(150)]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(500)]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Event date is required.")]
    [JsonPropertyName("date")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Location is required.")]
    [StringLength(150)]
    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("organiserId")]
    public int OrganiserId { get; set; }

    [JsonPropertyName("categoryId")]
    public int? CategoryId { get; set; }

    public int TotalEnrolments { get; set; }

    [JsonPropertyName("bannerImageUrl")]
    public string? BannerImageUrl { get; set; }

    [JsonIgnore]
    public IFormFile? BannerFile { get; set; }
}