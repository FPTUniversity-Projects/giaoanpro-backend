namespace giaoanpro_backend.Application.DTOs.Responses.Subscriptions
{
	public class GetMyHistorySubscriptionResponse
	{
		public Guid Id { get; set; }
		public Guid PlanId { get; set; }
		public string PlanName { get; set; } = null!;
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Status { get; set; } = null!;
	}
}
