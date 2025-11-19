using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class QuestionAttribute : AuditableEntity, ISoftDeleteEntity
	{
		public Guid QuestionId { get; set; }
		public Guid AttributeId { get; set; }

		// Navigation properties
		public virtual Question Question { get; set; } = null!;
		public virtual Attribute Attribute { get; set; } = null!;

		// ISoftDeleteEntity implementation
		public DateTime? DeletedAt { get; set; }
	}
}
