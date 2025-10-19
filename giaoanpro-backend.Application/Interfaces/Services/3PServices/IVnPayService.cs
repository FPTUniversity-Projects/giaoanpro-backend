using giaoanpro_backend.Application.DTOs.Requests.VnPays;
using giaoanpro_backend.Application.DTOs.Responses.Subscriptions;
using giaoanpro_backend.Application.DTOs.Responses.VnPays;
using Microsoft.AspNetCore.Http;

namespace giaoanpro_backend.Application.Interfaces.Services._3PServices
{
	public interface IVnPayService
	{
		public Task<SubscriptionCheckoutResponse> CreatePaymentUrlAsync(HttpContext context, VnPaymentRequest request);
		public Task<VnPaymentResponse> GetPaymentResponseAsync(IQueryCollection queryParameters);

	}
}
