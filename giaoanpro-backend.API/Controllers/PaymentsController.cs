using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Payments;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Text.Json;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentsController : BaseApiController
	{
		private readonly IPaymentService _paymentService;
		private readonly ILogger<PaymentsController> _logger;
		private readonly IConfiguration _configuration;

		public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger, IConfiguration configuration)
		{
			_paymentService = paymentService;
			_logger = logger;
			_configuration = configuration;
		}

		[HttpGet("my-history")]
		[Authorize]
		[ProducesResponseType(typeof(BaseResponse<List<GetPaymentResponse>>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<List<GetPaymentResponse>>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<List<GetPaymentResponse>>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<List<GetPaymentResponse>>>> GetMyPaymentHistory()
		{
			var userId = GetCurrentUserId();
			var result = await _paymentService.GetPaymentHistoryByUserIdAsync(userId);
			return HandleResponse(result);
		}

		[HttpGet("{id:guid}")]
		[Authorize]
		[ProducesResponseType(typeof(BaseResponse<GetPaymentDetailResponse>), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(BaseResponse<GetPaymentDetailResponse>), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(BaseResponse<GetPaymentDetailResponse>), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(typeof(BaseResponse<GetPaymentDetailResponse>), StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<BaseResponse<GetPaymentDetailResponse>>> GetMyPaymentById([FromRoute] Guid id)
		{
			var userId = GetCurrentUserId();
			var result = await _paymentService.GetUserPaymentByIdAsync(id, userId);
			return HandleResponse(result);
		}

		[HttpGet("vnpay-return")]
		public async Task<IActionResult> HandleVnPayCallback()
		{
			// Call service to let backend process the VNPay return (update DB, statuses, etc.)
			var result = await _paymentService.GetVnPayReturnResponseAsync(Request.Query);

			// Get frontend base URL from configuration (fallback to localhost if missing)
			string frontendUrl = _configuration["Front-end:webUrl"] ?? "http://localhost:3000";

			// Keys to forward from VNPay query to front-end. Exclude secure hash for safety.
			var forwardKeys = new[]
			{
				"vnp_Amount",
				"vnp_BankCode",
				"vnp_BankTranNo",
				"vnp_CardType",
				"vnp_OrderInfo",
				"vnp_PayDate",
				"vnp_ResponseCode",
				"vnp_TmnCode",
				"vnp_TransactionNo",
				"vnp_TransactionStatus",
				"vnp_TxnRef"
			};

			var qsParts = new List<string>();

			// Helper to read and add query param if present
			void TryAddQuery(string key)
			{
				if (Request.Query.TryGetValue(key, out var val) && !StringValues.IsNullOrEmpty(val))
				{
					qsParts.Add($"{key}={Uri.EscapeDataString(val.ToString())}");
				}
			}

			foreach (var k in forwardKeys)
			{
				TryAddQuery(k);
			}

			// Include subscriptionId from service payload when available
			if (result != null && result.Payload != null && result.Payload.SubscriptionId != Guid.Empty)
			{
				qsParts.Add($"subscriptionId={result.Payload.SubscriptionId}");
			}

			// Also add a normalized amount (divide VNPay value by 100 if numeric) for front-end convenience
			if (Request.Query.TryGetValue("vnp_Amount", out var vnpAmt) && !StringValues.IsNullOrEmpty(vnpAmt))
			{
				if (long.TryParse(vnpAmt.ToString(), out var raw))
				{
					var display = (raw / 100m).ToString("0.##");
					qsParts.Add($"amount={Uri.EscapeDataString(display)}");
				}
				else
				{
					qsParts.Add($"amount={Uri.EscapeDataString(vnpAmt.ToString())}");
				}
			}

			string redirectUrl;
			if (result!.Success)
			{
				// Redirect to frontend success page with important params so client can display details
				var queryString = qsParts.Count > 0 ? "?" + string.Join("&", qsParts) : string.Empty;
				redirectUrl = $"{frontendUrl}/payment/success{queryString}";
			}
			else
			{
				// On failure include error message and forwarded params
				qsParts.Add($"error={Uri.EscapeDataString(result.Message ?? "Unknown error")}");
				var queryString = "?" + string.Join("&", qsParts);
				redirectUrl = $"{frontendUrl}/payment/failed{queryString}";
			}

			return Redirect(redirectUrl);
		}

		[HttpGet("vnpay-ipn")]
		public async Task<ActionResult> HandleVnPayIpn()
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
			_logger.LogInformation("VNPay IPN callback received: {query}", JsonSerializer.Serialize(queryParameters));

			var result = await _paymentService.ProcessVnPayPaymentCallbackAsync(queryParameters);

			if (result.Success)
			{
				return Content("OK");
			}

			return BadRequest(result.Message ?? "Failed to process IPN");
		}
	}
}
