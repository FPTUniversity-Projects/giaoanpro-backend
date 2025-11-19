using giaoanpro_backend.Application.DTOs.Requests.Bases;

namespace giaoanpro_backend.Application.DTOs.Requests.Activities
{
	public class GetActivitiesQuery : PagingAndSortingQuery
	{
		public Guid? LessonPlanId { get; set; }
		public Guid? ParentId { get; set; }
		//public int PageNumber { get; set; } = 1;
		//public int PageSize { get; set; } = 10;
	}
}
