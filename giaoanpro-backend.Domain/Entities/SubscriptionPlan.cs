using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class SubscriptionPlan : AuditableEntity, ISoftDeleteEntity
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public int DurationInDays { get; set; }
		public bool IsActive { get; set; }

		// Limits
		public int MaxLessonPlans { get; set; }
		public int MaxPromptsPerDay { get; set; }

		// Navigation properties
		public ICollection<Subscription> UserSubscriptions { get; set; } = new List<Subscription>();

		// ISoftDeleteEntity implementation
		public DateTime? DeletedAt { get; set; }
	}
}
