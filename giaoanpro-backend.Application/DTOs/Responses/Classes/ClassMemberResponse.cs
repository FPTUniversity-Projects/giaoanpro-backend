namespace giaoanpro_backend.Application.DTOs.Responses.Classes
{
	public class ClassMemberResponse
	{
		public Guid Id { get; set; }
		public string FullName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
	}
}
