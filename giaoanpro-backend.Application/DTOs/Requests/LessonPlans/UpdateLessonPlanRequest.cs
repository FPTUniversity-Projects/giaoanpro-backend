namespace giaoanpro_backend.Application.DTOs.Requests.LessonPlans
{
	public class UpdateLessonPlanRequest
	{
		public Guid SubjectId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Objective { get; set; } = string.Empty;
		public string Note { get; set; } = string.Empty;
	}
}
