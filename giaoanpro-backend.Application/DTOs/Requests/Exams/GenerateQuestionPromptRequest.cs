using System;
using System.Text.Json.Serialization;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.DTOs.Requests.Exams
{
    public class GenerateQuestionPromptRequest
    {
        public Guid MatrixId { get; set; }
        public int Count { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AwarenessLevel AwarenessLevel { get; set; } = AwarenessLevel.Understand;
    }
}
