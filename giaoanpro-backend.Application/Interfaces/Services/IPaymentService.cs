using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.VnPays;
using Microsoft.AspNetCore.Http;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface IPaymentService
	{
		public Task<BaseResponse<bool>> ProcessVnPayPaymentCallbackAsync(IQueryCollection queryParameters);
		public Task<BaseResponse<VnPayReturnResponse>> GetVnPayReturnResponseAsync(IQueryCollection queryParameters);
	}
}
