using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.DTOs.Responses.Users
{
	public class GetUserLookupResponse
	{
		public Guid Id { get; set; }
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public UserRole Role { get; set; }
	}
}
