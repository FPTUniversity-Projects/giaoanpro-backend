namespace giaoanpro_backend.Application.DTOs.Requests.Users
{
	public class GetUserLookupRequest
	{
		public bool IncludeInactive { get; set; } = false;
		public bool IncludeTeacherOnly { get; set; } = false;
	}
}
