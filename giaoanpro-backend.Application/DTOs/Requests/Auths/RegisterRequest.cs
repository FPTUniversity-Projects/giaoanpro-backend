namespace giaoanpro_backend.Application.DTOs.Requests.Auths
{
	public class RegisterRequest
	{
		public string Username { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
	}
}
