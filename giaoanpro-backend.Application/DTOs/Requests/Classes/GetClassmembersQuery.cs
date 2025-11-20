using giaoanpro_backend.Application.DTOs.Requests.Bases;

namespace giaoanpro_backend.Application.DTOs.Requests.Classes
{
	public class GetClassmembersQuery : PagingAndSortingQuery
	{
		// Optional search term to filter members by full name or email
		public string? Search { get; set; }

		// Optional role filter (e.g., "Student", "Teacher") - matched case-insensitively against user's role string
		public string? Role { get; set; }
	}
}
