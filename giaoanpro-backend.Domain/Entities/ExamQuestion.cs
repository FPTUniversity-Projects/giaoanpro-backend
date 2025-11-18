using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class ExamQuestion : AuditableEntity, ISoftDeleteEntity
	{
		public Guid ExamId { get; set; }
		public Guid QuestionId { get; set; }
		public int SequenceNumber { get; set; }

		// Navigation properties
		public virtual Exam Exam { get; set; } = null!;
		public virtual Question Question { get; set; } = null!;

		// ISoftDeleteEntity implementation
		public DateTime? DeletedAt { get; set; }
	}
}
