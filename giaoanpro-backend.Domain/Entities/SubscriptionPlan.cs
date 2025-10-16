using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class SubscriptionPlan : AuditableEntity
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public int DurationInDays { get; set; }

		// Navigation properties
		public ICollection<Subscription> UserSubscriptions { get; set; } = new List<Subscription>();
	}
}
