using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class Subject : AuditableEntity
	{
		public Guid Id { get; set; }
		public Guid GradeId { get; set; }
		public Guid SyllabusId { get; set; }
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		// Navigation property
		public virtual Syllabus Syllabus { get; set; } = null!;
		public virtual Grade Grade { get; set; } = null!;
		public virtual ICollection<LessonPlan> LessonPlans { get; set; } = new List<LessonPlan>();
		public virtual ICollection<ExamMatrix> ExamMatrices { get; set; } = new List<ExamMatrix>();
	}
}
