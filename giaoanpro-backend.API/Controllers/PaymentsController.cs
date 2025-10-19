using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentsController : ControllerBase
	{
		private readonly IPaymentService _paymentService;

		public PaymentsController(IPaymentService paymentService)
		{
			_paymentService = paymentService;
		}

		[HttpGet("vnpay-callback")]
		public async Task<IActionResult> HandleVnPayCallback([FromQuery] IQueryCollection queryParameters)
		{
			var result = await _paymentService.ProcessVnPayPaymentCallbackAsync(queryParameters);
			return result.Success ? Ok(result) : BadRequest(result);
		}
	}
}
