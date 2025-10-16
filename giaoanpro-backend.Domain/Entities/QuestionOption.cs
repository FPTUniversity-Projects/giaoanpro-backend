using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class QuestionOption : AuditableEntity
	{
		public Guid Id { get; set; }
		public Guid QuestionId { get; set; }
		public string Text { get; set; } = string.Empty;
		public bool IsCorrect { get; set; }

		// Navigation property
		public virtual Question Question { get; set; } = null!;
	}
}
