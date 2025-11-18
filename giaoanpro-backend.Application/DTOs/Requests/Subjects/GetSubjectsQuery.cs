using giaoanpro_backend.Application.DTOs.Requests.Bases;

namespace giaoanpro_backend.Application.DTOs.Requests.Subjects
{
	public class GetSubjectsQuery : PagingAndSortingQuery
	{
		public string? Name { get; set; }
		public Guid? GradeId { get; set; }
	}
}
