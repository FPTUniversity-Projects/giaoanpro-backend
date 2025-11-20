using giaoanpro_backend.Domain.Bases;
using giaoanpro_backend.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace giaoanpro_backend.Domain.Entities
{
	public class Material : AuditableEntity
	{
		public Guid Id { get; set; }
		public Guid ActivityId { get; set; }
		
		[MaxLength(255)]
		public string Title { get; set; } = string.Empty;
		
		public MaterialType Type { get; set; }
		
		[MaxLength(500)]
		public string Url { get; set; } = string.Empty;

		// Navigation property
		public virtual Activity Activity { get; set; } = null!;
	}
}
