using giaoanpro_backend.Domain.Bases;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Domain.Entities
{
	public class Activity : AuditableEntity
	{
		public Guid Id { get; set; }
		public Guid LessonPlanId { get; set; }
		public Guid? ParentId { get; set; }
		public ActivityType Type { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Objective { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public string Product { get; set; } = string.Empty;
		public string Implementation { get; set; } = string.Empty;

		// Navigation property
		public virtual LessonPlan LessonPlan { get; set; } = null!;
		public virtual Activity? Parent { get; set; }

		public virtual ICollection<Activity> Children { get; set; } = new List<Activity>();

		public virtual ICollection<Exam> Exams { get; set; } = new List<Exam>();
	}
}
