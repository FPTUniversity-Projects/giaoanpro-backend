using giaoanpro_backend.Application.DTOs.Requests.Bases;

namespace giaoanpro_backend.Application.DTOs.Requests.LessonPlans
{
	public class GetLessonPlansQuery : PagingAndSortingQuery
	{
		public string? Title { get; set; }
		public Guid? SubjectId { get; set; }
		public Guid? UserId { get; set; }
		//public int PageNumber { get; set; } = 1;
		//public int PageSize { get; set; } = 10;
	}
}
