namespace giaoanpro_backend.Application.DTOs.Responses.Questions
{
	public class GetQuestionResponse
	{
		public Guid Id { get; set; }
		public string Text { get; set; } = string.Empty;
		public string QuestionType { get; set; } = string.Empty;
		public string DifficultyLevel { get; set; } = string.Empty;
		public string AwarenessLevel { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
		public List<QuestionOptionDto> Options { get; set; } = new();
	}


	
}
