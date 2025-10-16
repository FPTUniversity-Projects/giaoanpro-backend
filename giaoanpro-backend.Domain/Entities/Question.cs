using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class Question : AuditableEntity
	{
		public Guid Id { get; set; }
		public Guid BankId { get; set; }
		public Guid PromptId { get; set; }
		public string Text { get; set; } = string.Empty;

		// Navigation properties
		public virtual QuestionBank Bank { get; set; } = null!;
		public virtual PromptLog Prompt { get; set; } = null!;
		public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
		public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
		public virtual ICollection<AttemptDetail> AttemptDetails { get; set; } = new List<AttemptDetail>();
		public virtual ICollection<QuestionAttribute> QuestionAttributes { get; set; } = new List<QuestionAttribute>();
	}
}
