using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class QuestionBank : AuditableEntity
	{
		public Guid Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		// Navigation properties
		public ICollection<Question> Questions { get; set; } = new List<Question>();
	}
}
