using giaoanpro_backend.Domain.Bases;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Domain.Entities
{
	public class Subscription : AuditableEntity, ISoftDeleteEntity
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid PlanId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public SubscriptionStatus Status { get; set; }

		// Navigation properties
		public virtual User User { get; set; } = null!;
		public virtual SubscriptionPlan Plan { get; set; } = null!;

		// ISoftDeleteEntity implementation
		public DateTime? DeletedAt { get; set; }
	}
}
