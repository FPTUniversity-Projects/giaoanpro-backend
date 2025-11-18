namespace giaoanpro_backend.Application.DTOs.Responses.Subscriptions.Shared
{
	public class SubscriptionPlanDetailResponse
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = null!;
		public string Description { get; set; } = null!;
		public decimal Price { get; set; }
		public int DurationInDays { get; set; }
		public int MaxLessonPlans { get; set; }
		public int MaxPromptsPerDay { get; set; }
	}
}
