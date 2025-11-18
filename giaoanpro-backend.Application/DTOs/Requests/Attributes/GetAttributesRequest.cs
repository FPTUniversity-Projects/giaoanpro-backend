using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.DTOs.Requests.Attributes
{
	public class GetAttributesRequest
	{
		public int PageNumber { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public string? Type { get; set; }
		public TypeValue? Value { get; set; }
	}
}
