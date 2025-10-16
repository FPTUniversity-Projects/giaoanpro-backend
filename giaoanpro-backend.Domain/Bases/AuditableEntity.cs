namespace giaoanpro_backend.Domain.Bases
{
	public abstract class AuditableEntity
	{
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
