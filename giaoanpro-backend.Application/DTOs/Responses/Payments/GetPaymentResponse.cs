using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.DTOs.Responses.Payments
{
	public class GetPaymentResponse
	{
		public Guid Id { get; set; }
		public Guid SubscriptionId { get; set; }

		public decimal AmountPaid { get; set; }
		public DateTime PaymentDate { get; set; }
		public PaymentStatus Status { get; set; }
		public string PaymentMethod { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
	}
}
