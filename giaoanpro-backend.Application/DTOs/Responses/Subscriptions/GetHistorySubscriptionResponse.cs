namespace giaoanpro_backend.Application.DTOs.Responses.Subscriptions
{
	public class GetHistorySubscriptionResponse
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid PlanId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public string Status { get; set; } = null!;
	}
}
