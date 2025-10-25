using giaoanpro_backend.Domain.Bases;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Domain.Entities
{
	public class Payment : AuditableEntity, ISoftDeleteEntity
	{
		public Guid Id { get; set; }

		public Guid SubscriptionId { get; set; }

		public decimal AmountPaid { get; set; }
		public DateTime PaymentDate { get; set; }
		public PaymentStatus Status { get; set; }
		public string PaymentMethod { get; set; } = string.Empty;
		public string GatewayTransactionId { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string VnpResponseCode { get; set; } = string.Empty;

		// Navigation property
		public virtual Subscription Subscription { get; set; } = null!;
		public DateTime? DeletedAt { get; set; }
	}
}
