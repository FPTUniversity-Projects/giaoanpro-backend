namespace giaoanpro_backend.Application.DTOs.Requests.VnPays
{
	public class VnPaymentRequest
	{
		public Guid SubscriptionId { get; set; }
		public Guid PaymentId { get; set; }
		public int Amount { get; set; } = 0;
	}
}
