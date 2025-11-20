using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.DTOs.Responses.Materials
{
	public class MaterialResponse
	{
		public Guid Id { get; set; }
		public Guid ActivityId { get; set; }
		public string Title { get; set; } = string.Empty;
		public MaterialType Type { get; set; }
		public string Url { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
