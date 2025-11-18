namespace giaoanpro_backend.Application.DTOs.Responses.Subscriptions.Shared
{
	public class UserDetailResponse
	{
		public Guid Id { get; set; }
		public string Email { get; set; } = null!;
		public string FullName { get; set; } = null!;
	}
}
