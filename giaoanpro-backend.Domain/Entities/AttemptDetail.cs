using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class AttemptDetail : AuditableEntity
	{
		public Guid Id { get; set; }
		public Guid AttemptId { get; set; }
		public Guid QuestionId { get; set; }
		public string Answer { get; set; } = string.Empty;
		public bool IsCorrect { get; set; }
		public int Score { get; set; }
		public string Feedback { get; set; } = string.Empty;

		// Navigation properties
		public virtual Attempt Attempt { get; set; } = null!;
		public virtual Question Question { get; set; } = null!;
	}
}
