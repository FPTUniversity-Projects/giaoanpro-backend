using giaoanpro_backend.Domain.Bases;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Domain.Entities
{
	public class Attempt : AuditableEntity
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid ExamId { get; set; }
		public DateTime StartedAt { get; set; }
		public DateTime? CompletedAt { get; set; }
		public int FinalScore { get; set; }

		// Use AttemptStatus enum for lifecycle state
		public AttemptStatus Status { get; set; } = AttemptStatus.InProgress;

		// Auto-scored (MCQ) and manual components
		public decimal? AutoScore { get; set; }
		public decimal? ManualScore { get; set; }

		// Navigation properties
		public virtual User User { get; set; } = null!;
		public virtual Exam Exam { get; set; } = null!;
		public virtual ICollection<AttemptDetail>? AttemptDetails { get; set; } = new List<AttemptDetail>();
	}
}
