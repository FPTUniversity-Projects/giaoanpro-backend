using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class ClassMember : AuditableEntity
	{
		public Guid StudentId { get; set; }
		public Guid ClassId { get; set; }

		// Navigation properties
		public virtual User Student { get; set; } = null!;
		public virtual Class Class { get; set; } = null!;
	}
}
