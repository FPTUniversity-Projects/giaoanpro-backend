namespace giaoanpro_backend.Application.DTOs.Requests.Subscriptions
{
	public class SubscriptionCheckoutRequest
	{
		public Guid PlanId { get; set; }
		public Guid? SubscriptionId { get; set; }
	}
}
