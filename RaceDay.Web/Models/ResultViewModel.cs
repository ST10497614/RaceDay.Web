
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Web.Models
{
    public class ResultViewModel
    {
        [JsonPropertyName("resultId")]
        public int ResultId { get; set; }

        [JsonPropertyName("enrolmentId")]
        [Required]
        public int EnrolmentId { get; set; }

        [JsonPropertyName("finishTime")]
        public TimeOnly? FinishTime { get; set; }

        [JsonPropertyName("position")]
        public int? Position { get; set; }
    }
}
