using System.Text.Json.Serialization;

namespace RaceDay.Web.Models
{
    public class EnrolmentViewModel
    {
        [JsonPropertyName("enrolmentId")]
        public int EnrolmentId { get; set; }

        [JsonPropertyName("participantId")]
        public int ParticipantId { get; set; }

        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("enrolmentDate")]
        public DateTime? EnrolmentDate { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("category")]
        public CategoryViewModel? Category { get; set; }
    }
}