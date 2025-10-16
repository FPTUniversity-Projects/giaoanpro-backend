using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class Semester : AuditableEntity
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		// Navigation properties
		public ICollection<Class> Classes { get; set; } = new List<Class>();
	}
}
