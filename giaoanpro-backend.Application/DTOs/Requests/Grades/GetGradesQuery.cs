using giaoanpro_backend.Application.DTOs.Requests.Bases;

namespace giaoanpro_backend.Application.DTOs.Requests.Grades
{
	public class GetGradesQuery : PagingAndSortingQuery
	{
		public int? Level { get; set; }
	}
}
