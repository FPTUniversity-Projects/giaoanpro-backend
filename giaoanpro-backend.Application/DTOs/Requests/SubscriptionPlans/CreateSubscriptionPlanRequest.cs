namespace giaoanpro_backend.Application.DTOs.Requests.SubscriptionPlans
{
	public class CreateSubscriptionPlanRequest
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public int DurationInDays { get; set; }
	}
}
