using giaoanpro_backend.Application.DTOs.Responses.Exams;
using giaoanpro_backend.Application.DTOs.Responses.Materials;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.DTOs.Responses.Activities
{
	public class ActivityResponse
	{
		public Guid Id { get; set; }
		public Guid LessonPlanId { get; set; }
		public string LessonPlanTitle { get; set; } = string.Empty;
		public Guid? ParentId { get; set; }
		public ActivityType Type { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Objective { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public string Product { get; set; } = string.Empty;
		public string Implementation { get; set; } = string.Empty;
		public int ChildrenCount { get; set; }
		public List<ExamSummaryResponse> Exams { get; set; } = new List<ExamSummaryResponse>();
		public List<MaterialResponse> Materials { get; set; } = new List<MaterialResponse>();
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
