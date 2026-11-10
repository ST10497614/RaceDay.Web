using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Web.Models
{
    public class CategoryViewModel
    {
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("eventId")]
        [Required]
        public int EventId { get; set; }

        [JsonPropertyName("categoryName")]
        [Required]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("distanceKm")]
        [Range(0.1, 500)]
        public decimal DistanceKm { get; set; }

        [JsonPropertyName("maxParticipants")]
        [Range(1, 100000)]
        public int MaxParticipants { get; set; }
    }
}