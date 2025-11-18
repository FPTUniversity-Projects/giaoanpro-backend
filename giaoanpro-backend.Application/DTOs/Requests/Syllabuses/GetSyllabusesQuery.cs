using giaoanpro_backend.Application.DTOs.Requests.Bases;

namespace giaoanpro_backend.Application.DTOs.Requests.Syllabuses
{
	public class GetSyllabusesQuery : PagingAndSortingQuery
	{
		public string? Name { get; set; }
		public Guid? SubjectId { get; set; }
	}
}
