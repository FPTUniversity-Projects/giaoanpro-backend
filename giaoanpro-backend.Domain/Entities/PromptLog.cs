using giaoanpro_backend.Domain.Bases;

namespace giaoanpro_backend.Domain.Entities
{
	public class PromptLog : AuditableEntity
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public string Prompt { get; set; } = string.Empty;
		public string Response { get; set; } = string.Empty;

		// Navigation property
		public virtual User User { get; set; } = null!;
	}
}
