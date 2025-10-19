using giaoanpro_backend.Application.DTOs.Requests.VnPays;
using giaoanpro_backend.Application.DTOs.Responses.Subscriptions;
using giaoanpro_backend.Application.DTOs.Responses.VnPays;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;
using giaoanpro_backend.Infrastructure.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace giaoanpro_backend.Infrastructure._3PServices
{
	public class VnPayService : IVnPayService
	{
		private readonly IConfiguration _config;

		public VnPayService(IConfiguration config)
		{
			_config = config;
		}

		//private readonly IWebHostEnvironment _env;

		public async Task<SubscriptionCheckoutResponse> CreatePaymentUrlAsync(HttpContext context, VnPaymentRequest request)
		{
			//var returnUrl = _webHostEnvironment.IsDevelopment()
			//    ? _config["VnPay:ReturnUrl"]
			//    : _config["VnPay:ReturnUrlProduct"];
			var returnUrl = _config["VnPay:ReturnUrl"] ?? string.Empty;

			var vnpay = new VnPayLibrary();
			vnpay.AddRequestData("vnp_Version", _config["VnPay:Version"] ?? string.Empty);
			vnpay.AddRequestData("vnp_Command", _config["VnPay:Command"] ?? string.Empty);
			vnpay.AddRequestData("vnp_TmnCode", _config["VnPay:TmnCode"] ?? string.Empty);

			// ensure null-coalesce happens BEFORE multiplying by 100 (VNPay expects amount in smallest unit)
			vnpay.AddRequestData("vnp_Amount", ((request.Amount) * 100).ToString());

			// Use utc now for create date
			vnpay.AddRequestData("vnp_CreateDate", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
			vnpay.AddRequestData("vnp_CurrCode", _config["VnPay:CurrCode"] ?? string.Empty);
			var ipAddress = await Utils.GetIpAddressAsync(context);
			vnpay.AddRequestData("vnp_IpAddr", ipAddress ?? string.Empty);
			vnpay.AddRequestData("vnp_Locale", _config["VnPay:Locale"] ?? string.Empty);

			// include subscription id and txn id in order info for later reconciliation
			var orderInfo = $"Thanh toan don hang: {request.SubscriptionId}. So tien {request.Amount} {_config["VnPay:CurrCode"]}";
			vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
			vnpay.AddRequestData("vnp_OrderType", "190000"); // intertainment and education
			vnpay.AddRequestData("vnp_ReturnUrl", returnUrl ?? string.Empty);

			// Use GUID string for txn reference (matches PaymentTransaction.Id)
			vnpay.AddRequestData("vnp_TxnRef", request.PaymentId.ToString());

			string paymentUrl = vnpay.CreateRequestUrl(_config["VnPay:PaymentUrl"] ?? string.Empty, _config["VnPay:HashSecret"] ?? string.Empty);
			return new SubscriptionCheckoutResponse { PaymentUrl = paymentUrl, SubscriptionId = request.SubscriptionId };
		}

		public async Task<VnPaymentResponse> GetPaymentResponseAsync(IQueryCollection queryParameters)
		{
			var vnpay = new VnPayLibrary();
			foreach (var item in queryParameters)
			{
				var key = item.Key;
				var value = item.Value;
				if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
				{
					vnpay.AddResponseData(key, value.ToString());
				}
			}

			if (!Guid.TryParse(vnpay.GetResponseData("vnp_TxnRef"), out var txnRef))
			{
				return new VnPaymentResponse
				{
					IsSuccess = false,
					Message = "Invalid or missing vnp_TxnRef in VNPay response"
				};
			}

			// Get secure hash from response data
			var secureHashString = vnpay.GetResponseData("vnp_SecureHash");
			if (string.IsNullOrEmpty(secureHashString))
			{
				secureHashString = queryParameters.TryGetValue("vnp_SecureHash", out var sec) ? sec.ToString() : string.Empty;
			}
			if (string.IsNullOrEmpty(secureHashString))
			{
				return new VnPaymentResponse
				{
					IsSuccess = false,
					Message = "Missing vnp_SecureHash in VNPay response"
				};
			}

			// Validate signature
			var secret = _config["VnPay:HashSecret"] ?? string.Empty;
			bool checkSignature = vnpay.ValidateSignature(secureHashString, secret);
			if (!checkSignature)
			{
				return new VnPaymentResponse
				{
					IsSuccess = false,
					Message = "Signature validation failed"
				};
			}

			var vnp_TranNo = vnpay.GetResponseData("vnp_TransactionNo");
			var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
			var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");
			var rawAmountStr = vnpay.GetResponseData("vnp_Amount");

			// consider success only if signature is valid and response code is "00"
			var success = checkSignature && string.Equals(vnp_ResponseCode, "00", StringComparison.OrdinalIgnoreCase);

			return new VnPaymentResponse
			{
				IsSuccess = success,
				Message = success ? "Payment successful" : "Payment failed",
				PaymentId = txnRef,
				TransactionNo = vnp_TranNo,
				PaymentInfo = vnp_OrderInfo,
				ResponseCode = vnp_ResponseCode,
				Amount = decimal.TryParse(rawAmountStr, out var amt) ? amt / 100 : 0m
			};
		}
	}
}
