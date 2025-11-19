using giaoanpro_backend.Application.DTOs.Requests.Bases;

namespace giaoanpro_backend.Application.DTOs.Requests.Users
{
	public class GetUsersQuery : PagingAndSortingQuery
	{
		public string? Search { get; set; }
		public bool IncludeInactive { get; set; } = false;
		public bool TeacherOnly { get; set; } = false;
	}
}
