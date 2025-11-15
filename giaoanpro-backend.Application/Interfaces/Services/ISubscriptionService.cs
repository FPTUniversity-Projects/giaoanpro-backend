using giaoanpro_backend.Application.DTOs.Requests.Subscriptions;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Subscriptions;
using Microsoft.AspNetCore.Http;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface ISubscriptionService
	{
		public Task<BaseResponse<PagedResult<GetHistorySubscriptionResponse>>> GetSubscriptionHistoryByUserIdAsync(Guid userId, GetMySubscriptionHistoryQuery query);
		public Task<BaseResponse<PagedResult<GetHistorySubscriptionResponse>>> GetSubscriptionsAsync(GetSubscriptionsQuery query);
		public Task<BaseResponse<GetSubscriptionResponse>> GetCurrentAccessSubscriptionByUserIdAsync(Guid userId);
		public Task<BaseResponse<GetSubscriptionDetailResponse>> GetUserSubscriptionByIdAsync(Guid subscriptionId, Guid userId);
		public Task<BaseResponse<SubscriptionCheckoutResponse>> CreateSubscriptionCheckoutSessionAsync(Guid userId, SubscriptionCheckoutRequest request, HttpContext httpContext);
		public Task<BaseResponse<string>> CancelSubscriptionAsync(Guid subscriptionId, Guid userId);
	}
}
