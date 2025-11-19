using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.DTOs.Responses.Users
{
	public class GetUserResponse
	{
		public Guid Id { get; set; }
		public string Email { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string Username { get; set; } = string.Empty;
		public bool IsActive { get; set; }
		public UserRole Role { get; set; }
	}
}
