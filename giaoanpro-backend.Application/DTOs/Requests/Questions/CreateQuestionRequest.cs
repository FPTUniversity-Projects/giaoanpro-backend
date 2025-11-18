using giaoanpro_backend.Domain.Enums;
using System.Text.Json.Serialization;

namespace giaoanpro_backend.Application.DTOs.Requests.Questions
{
	public class CreateQuestionRequest
	{
		public string Text { get; set; } = string.Empty;
		public List<CreateQuestionOptionDto> Options { get; set; } = new();
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public QuestionType QuestionType { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DifficultyLevel DifficultyLevel { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public AwarenessLevel AwarenessLevel { get; set; }
	}

	public class CreateQuestionOptionDto
	{
		public string Text { get; set; } = string.Empty;
		public bool IsCorrect { get; set; }
	}
}
