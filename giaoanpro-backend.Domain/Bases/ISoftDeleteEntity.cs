namespace giaoanpro_backend.Domain.Bases
{
	public interface ISoftDeleteEntity
	{
		public DateTime? DeletedAt { get; set; }
	}
}
