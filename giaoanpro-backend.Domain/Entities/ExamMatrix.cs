using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class ExamMatrix : AuditableEntity
	{
		public Guid Id { get; set; }
		public Guid SubjectId { get; set; }
		public Guid LessonPlanId { get; set; }
		public string Topic { get; set; } = string.Empty;
		public int NumberOfQuestions { get; set; }
		public int MarksPerQuestion { get; set; }

		// Navigation properties
		public virtual Subject Subject { get; set; } = null!;
		public virtual LessonPlan LessonPlan { get; set; } = null!;
		public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
	}
}
