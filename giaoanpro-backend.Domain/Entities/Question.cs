using giaoanpro_backend.Domain.Bases;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Domain.Entities
{
	public class Question : AuditableEntity, ISoftDeleteEntity
	{
		public Guid Id { get; set; }
		public string Text { get; set; } = string.Empty;
		public QuestionType QuestionType { get; set; }
		public DifficultyLevel DifficultyLevel { get; set; }
		public AwarenessLevel AwarenessLevel { get; set; }
		public Guid LessonPlanId { get; set; }
		public virtual LessonPlan LessonPlan { get; set; } = null!;

		// Navigation properties
		public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
		public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
		public virtual ICollection<AttemptDetail> AttemptDetails { get; set; } = new List<AttemptDetail>();
        public DateTime? DeletedAt { get; set; }
    }
}
