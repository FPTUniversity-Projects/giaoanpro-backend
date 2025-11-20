namespace giaoanpro_backend.Application.DTOs.Responses.LessonPlans
{
	public class SubscriptionInfoResponse
	{
		public Guid SubscriptionId { get; set; }
		public string PlanName { get; set; } = string.Empty;
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int MaxLessonPlans { get; set; }
		public int UsedLessonPlans { get; set; }
		public int AvailableLessonPlans { get; set; }
		public bool IsActive { get; set; }
	}
}
