namespace giaoanpro_backend.Application.DTOs.Responses.Questions
{
    public class QuestionAttributeDto
    {
        public Guid AttributeId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
