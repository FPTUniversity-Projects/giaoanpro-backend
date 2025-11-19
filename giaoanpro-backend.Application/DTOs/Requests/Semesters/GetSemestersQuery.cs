using giaoanpro_backend.Application.DTOs.Requests.Bases;

namespace giaoanpro_backend.Application.DTOs.Requests.Semesters
{
	public class GetSemestersQuery : PagingAndSortingQuery
	{
		public string? Name { get; set; }
		public DateTime? StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public bool? IsActive { get; set; } // Filter by current date
	}
}
