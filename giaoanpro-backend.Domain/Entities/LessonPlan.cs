using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class LessonPlan : AuditableEntity
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public Guid SubjectId { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Objective { get; set; } = string.Empty;
		public string Note { get; set; } = string.Empty;

		// Navigation properties
		public virtual User User { get; set; } = null!;
		public virtual Subject Subject { get; set; } = null!;
		public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
		public virtual ICollection<ExamMatrix> ExamMatrices { get; set; } = new List<ExamMatrix>();
		public virtual ICollection<Question> Questions { get; set; } = [];
	}
}
