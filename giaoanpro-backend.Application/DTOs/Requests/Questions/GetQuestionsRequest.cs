using giaoanpro_backend.Domain.Enums;
using System.Text.Json.Serialization;

namespace giaoanpro_backend.Application.DTOs.Requests.Questions
{
	public class GetQuestionsRequest
	{
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public QuestionType? QuestionType { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public DifficultyLevel? DifficultyLevel { get; set; }
		[JsonConverter(typeof(JsonStringEnumConverter))]
		public AwarenessLevel? AwarenessLevel { get; set; }
		public string? SearchText { get; set; }
	}
}
