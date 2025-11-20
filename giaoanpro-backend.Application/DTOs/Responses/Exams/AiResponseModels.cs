using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Responses.Exams
{
    public class AiQuestionWrapperRaw
    {
        public List<AiRawQuestion>? Questions { get; set; }
    }

    public class AiRawQuestion
    {
        public string? Text { get; set; }
        public string? QuestionType { get; set; }
        public string? DifficultyLevel { get; set; }
        public string? AwarenessLevel { get; set; }
        public List<AiOptionRaw>? Options { get; set; }
    }

    public class AiOptionRaw
    {
        public string? Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}
