using giaoanpro_backend.Application.DTOs.Responses.Bases;

namespace giaoanpro_backend.Application.DTOs.Responses.LessonPlans
{
	public class LessonPlanPagedResult : PagedResult<LessonPlanResponse>
	{
		public SubscriptionInfoResponse? SubscriptionInfo { get; set; }

		public LessonPlanPagedResult(
			IEnumerable<LessonPlanResponse> items, 
			int pageNumber, 
			int pageSize, 
			int totalCount,
			SubscriptionInfoResponse? subscriptionInfo = null) 
			: base(items, pageNumber, pageSize, totalCount)
		{
			SubscriptionInfo = subscriptionInfo;
		}
	}
}
