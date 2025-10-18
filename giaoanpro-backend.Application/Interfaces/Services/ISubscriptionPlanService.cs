using giaoanpro_backend.Application.DTOs.Requests.SubscriptionPlans;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.SubscriptionPlans;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface ISubscriptionPlanService
	{
		public Task<BaseResponse<List<GetSubscriptionPlanResponse>>> GetAllSubscriptionPlansAsync();
		public Task<BaseResponse<GetSubscriptionPlanResponse>> GetSubscriptionPlanByIdAsync(Guid id);
		public Task<BaseResponse<string>> CreateSubscriptionPlanAsync(CreateSubscriptionPlanRequest request);
		public Task<BaseResponse<string>> UpdateSubscriptionPlanAsync(Guid id, UpdateSubscriptionPlanRequest request);
		public Task<BaseResponse<string>> DeleteSubscriptionPlanAsync(Guid id);
	}
}
