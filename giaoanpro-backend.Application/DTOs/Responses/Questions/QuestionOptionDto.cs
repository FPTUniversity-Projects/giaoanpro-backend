namespace giaoanpro_backend.Application.DTOs.Responses.Questions
{
    public class QuestionOptionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

}
