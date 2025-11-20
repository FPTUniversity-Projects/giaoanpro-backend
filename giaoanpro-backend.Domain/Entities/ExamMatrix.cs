using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class ExamMatrix : AuditableEntity, ISoftDeleteEntity
	{
		public Guid Id { get; set; }
		public Guid SubjectId { get; set; }
		public Guid LessonPlanId { get; set; }
		public string Name { get; set; } = string.Empty;
		public int TotalQuestions { get; set; }

		// Navigation properties
		public virtual Subject Subject { get; set; } = null!;
		public virtual LessonPlan LessonPlan { get; set; } = null!;
		public virtual ICollection<ExamMatrixDetail> Lines { get; set; } = new List<ExamMatrixDetail>();
		public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();

		// Soft-delete
		public DateTime? DeletedAt { get; set; }
	}
}
