namespace giaoanpro_backend.Application.DTOs.Requests.Auths
{
	public class LoginRequest
	{
		public string Email { get; set; } = null!;
		public string Password { get; set; } = null!;
	}
}
