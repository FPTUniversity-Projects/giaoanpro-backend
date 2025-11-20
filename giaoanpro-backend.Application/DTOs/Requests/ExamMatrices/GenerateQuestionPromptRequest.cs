namespace giaoanpro_backend.Application.DTOs.Requests.ExamMatrices
{
    public class GenerateQuestionPromptRequest
    {
        public int Count { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
