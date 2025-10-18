namespace giaoanpro_backend.Application.DTOs.Responses.Subscriptions
{
	public class SubscriptionCheckoutResponse
	{
		public Guid SubscriptionId { get; set; }
		public string PaymentUrl { get; set; } = null!;
	}
}
