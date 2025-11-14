namespace giaoanpro_backend.Application.DTOs.Requests.SubscriptionPlans
{
	public class UpdateSubscriptionPlanRequest
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public int DurationInDays { get; set; }
		public int MaxLessonPlans { get; set; }
		public int MaxPromptsPerDay { get; set; }
		public bool IsActive { get; set; }
	}
}
