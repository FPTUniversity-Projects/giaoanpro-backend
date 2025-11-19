using giaoanpro_backend.Application.DTOs.Requests.SubscriptionPlans;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.SubscriptionPlans;
using giaoanpro_backend.Application.DTOs.Responses.Subscriptions.Shared;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface ISubscriptionPlanService
	{
		public Task<BaseResponse<PagedResult<GetSubscriptionPlanResponse>>> GetSubscriptionPlansAsync(GetSubscriptionPlansQuery query);
		public Task<BaseResponse<GetSubscriptionPlanResponse>> GetSubscriptionPlanByIdAsync(Guid id);
		public Task<BaseResponse<List<SubscriptionPlanLookupResponse>>> GetSubscriptionPlanLookupsAsync();
		public Task<BaseResponse<List<SubscriptionPlanLookupResponse>>> GetSubscriptionPlanAdminLookupsAsync(bool isActiveOnly);
		public Task<BaseResponse<string>> CreateSubscriptionPlanAsync(CreateSubscriptionPlanRequest request);
		public Task<BaseResponse<string>> UpdateSubscriptionPlanAsync(Guid id, UpdateSubscriptionPlanRequest request);
		public Task<BaseResponse<string>> DeleteSubscriptionPlanAsync(Guid id);
		public Task<BaseResponse<List<SubscriptionPlanDetailResponse>>> GetPublicSubscriptionPlansAsync();
	}
}
