using giaoanpro_backend.Application.DTOs.Requests.Bases;

namespace giaoanpro_backend.Application.DTOs.Requests.Classes
{
	public class GetClassesQuery : PagingAndSortingQuery
	{
		public string? Name { get; set; }
		public Guid? TeacherId { get; set; }
		public Guid? GradeId { get; set; }
		public Guid? SemesterId { get; set; }
	}
}
