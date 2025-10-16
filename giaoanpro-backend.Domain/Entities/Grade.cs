using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class Grade : AuditableEntity
	{
		public Guid Id { get; set; }
		public int Level { get; set; }

		// Navigation property
		public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
		public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
	}
}
