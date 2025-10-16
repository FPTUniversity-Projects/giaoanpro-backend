using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class Class : AuditableEntity
	{
		public Guid Id { get; set; }
		public Guid TeacherId { get; set; }
		public Guid GradeId { get; set; }
		public Guid SemesterId { get; set; }
		public string Name { get; set; } = string.Empty;

		// Navigation properties
		public virtual User Teacher { get; set; } = null!;
		public virtual Grade Grade { get; set; } = null!;
		public virtual Semester Semester { get; set; } = null!;
		public virtual ICollection<ClassMember> Members { get; set; } = new List<ClassMember>();
	}
}
