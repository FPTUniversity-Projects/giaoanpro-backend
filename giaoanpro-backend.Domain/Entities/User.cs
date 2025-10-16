using giaoanpro_backend.Domain.Bases;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Domain.Entities
{
	public class User : AuditableEntity
	{
		public Guid Id { get; set; }
		public string Username { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public UserRole Role { get; set; } = UserRole.User;

		// Navigation properties
		public ICollection<Class> ClassesTaught { get; set; } = new List<Class>();
		public ICollection<LessonPlan> LessonPlans { get; set; } = new List<LessonPlan>();
		public ICollection<PromptLog> PromptLogs { get; set; } = new List<PromptLog>();
		public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
		public ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
		public ICollection<Exam> CreatedExams { get; set; } = new List<Exam>();
		public ICollection<ClassMember> ClassMemberships { get; set; } = new List<ClassMember>();
	}
}
