using giaoanpro_backend.Domain.Enums;
using System.Text.Json.Serialization;

namespace giaoanpro_backend.Application.DTOs.Requests.Questions
{
	public class UpdateQuestionRequest
	{
		public string Text { get; set; } = string.Empty;
		public List<UpdateQuestionOptionDto> Options { get; set; } = new();
		public Guid LessonPlanId { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public QuestionType QuestionType { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DifficultyLevel DifficultyLevel { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public AwarenessLevel AwarenessLevel { get; set; }
	}

	public class UpdateQuestionOptionDto
	{
		public Guid? Id { get; set; } // Null if new option
		public string Text { get; set; } = string.Empty;
		public bool IsCorrect { get; set; }
	}
}
