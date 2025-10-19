using giaoanpro_backend.Application.DTOs.Requests.Subscriptions;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Subscriptions;
using Microsoft.AspNetCore.Http;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface ISubscriptionService
	{
		public Task<BaseResponse<SubscriptionCheckoutResponse>> CreateSubscriptionCheckoutSessionAsync(SubscriptionCheckoutRequest request, HttpContext httpContext);
	}
}
