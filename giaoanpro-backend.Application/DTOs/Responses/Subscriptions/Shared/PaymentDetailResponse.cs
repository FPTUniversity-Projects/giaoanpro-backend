namespace giaoanpro_backend.Application.DTOs.Responses.Subscriptions.Shared
{
	public class PaymentDetailResponse
	{
		public Guid Id { get; set; }
		public Guid SubscriptionId { get; set; }
		public decimal Amount { get; set; }
		public DateTime PaymentDate { get; set; }
		public string PaymentMethod { get; set; } = null!;
		public string Status { get; set; } = null!;
	}
}
