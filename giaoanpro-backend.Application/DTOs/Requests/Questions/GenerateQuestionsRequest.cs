using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.DTOs.Requests.Questions
{
    public class GenerateQuestionsRequest
    {
        public Guid LessonPlanId { get; set; }
        public List<GenerateQuestionSpec> Specs { get; set; } = new();
    }

    public class GenerateQuestionSpec
    {
        public QuestionType QuestionType { get; set; }
        public DifficultyLevel DifficultyLevel { get; set; }
        public AwarenessLevel AwarenessLevel { get; set; }
        public int Count { get; set; } = 1; // clamped to max 10
    }
}
