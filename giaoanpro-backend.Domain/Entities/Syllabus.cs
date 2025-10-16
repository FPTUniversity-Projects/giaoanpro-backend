using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class Syllabus : AuditableEntity
	{
		public Guid Id { get; set; }
		public Guid SubjectId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		// Navigation properties
		public virtual Subject Subject { get; set; } = null!;
	}
}
