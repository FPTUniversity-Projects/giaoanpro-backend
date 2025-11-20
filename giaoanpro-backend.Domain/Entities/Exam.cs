using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class Exam : AuditableEntity, ISoftDeleteEntity
    {
		public Guid Id { get; set; }
		// MatrixId made required. NOTE: existing DB rows must be cleaned/truncated before applying migration that makes this column NOT NULL.
		public Guid MatrixId { get; set; }
		public Guid? ActivityId { get; set; }
		public Guid CreatorId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public int DurationMinutes { get; set; }

		// Navigation properties
		public virtual ExamMatrix Matrix { get; set; } = null!;
		public virtual Activity Activity { get; set; } = null!;
		public virtual User Creator { get; set; } = null!;
		public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
        public DateTime? DeletedAt { get; set; }
    }
}
