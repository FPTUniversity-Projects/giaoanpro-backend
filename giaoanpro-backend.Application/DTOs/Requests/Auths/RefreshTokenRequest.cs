namespace giaoanpro_backend.Application.DTOs.Requests.Auths
{
	public class RefreshTokenRequest
	{
		public required Guid UserId { get; set; }
		public required string RefreshToken { get; set; }
	}
}
