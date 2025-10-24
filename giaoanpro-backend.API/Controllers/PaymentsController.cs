using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

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
		public async Task<IActionResult> HandleVnPayCallback()
		{
			var result = await _paymentService.ProcessVnPayPaymentCallbackAsync(Request.Query);
			return result.Success ? Ok(result) : BadRequest(result);
		}

		// VNPay server-to-server notification (IPN). VNPay may POST form-encoded data.
		// AllowAnonymous because the call comes from VNPay external server.
		[HttpPost("vnpay-ipn")]
		[AllowAnonymous]
		public async Task<IActionResult> HandleVnPayIpn()
		{
			IQueryCollection queryParameters;

			// VNPay sometimes sends parameters as form content. Accept both form and query.
			if (Request.HasFormContentType)
			{
				var form = await Request.ReadFormAsync();
				var dict = new Dictionary<string, StringValues>(StringComparer.Ordinal);
				foreach (var kv in form)
				{
					dict[kv.Key] = kv.Value;
				}
				queryParameters = new QueryCollection(dict);
			}
			else
			{
				// fallback to query string
				queryParameters = Request.Query;
			}

			var result = await _paymentService.ProcessVnPayPaymentCallbackAsync(queryParameters);

			// VNPay expects a simple plain response. Return 200 OK when processing succeeded.
			// If needed, change the content body to match VNPay doc (some integrations expect "OK").
			if (result.Success)
			{
				return Content("OK");
			}

			// Return 400 so VNPay can know processing failed.
			return BadRequest(result.Message ?? "Failed to process IPN");
		}
	}
}
