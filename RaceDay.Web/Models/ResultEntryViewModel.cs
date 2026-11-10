using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RaceDay.Web.Models;

public class ResultEntryViewModel
{
    [Required]
    [JsonPropertyName("enrolmentId")]
    public int EnrolmentId { get; set; }

    [Required]
    [Range(1, 10000, ErrorMessage = "Position must be greater than zero.")]
    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("finishTime")]
    [DataType(DataType.Time)]
    public TimeOnly? FinishTime { get; set; }
}