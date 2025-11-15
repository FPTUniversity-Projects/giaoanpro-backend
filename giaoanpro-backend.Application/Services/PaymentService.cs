using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Payments;
using giaoanpro_backend.Application.DTOs.Responses.VnPays;
using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;
using giaoanpro_backend.Domain.Enums;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace giaoanpro_backend.Application.Services
{
	public class PaymentService : IPaymentService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IPaymentRepository _paymentRepository;
		private readonly IVnPayService _vnPayService;
		private readonly IMapper _mapper;

		public PaymentService(IUnitOfWork unitOfWork, IVnPayService vnPayService, IMapper mapper, IPaymentRepository paymentRepository)
		{
			_unitOfWork = unitOfWork;
			_vnPayService = vnPayService;
			_mapper = mapper;
			_paymentRepository = paymentRepository;
		}

		public async Task<BaseResponse<List<GetPaymentResponse>>> GetPaymentHistoryByUserIdAsync(Guid userId)
		{
			if (userId == Guid.Empty)
				return BaseResponse<List<GetPaymentResponse>>.Fail("Invalid user id.", ResponseErrorType.BadRequest);

			var paymentList = (await _paymentRepository.GetHistoryByUserIdAsync(userId)).ToList();

			if (paymentList.Count == 0)
			{
				return BaseResponse<List<GetPaymentResponse>>.Ok(new List<GetPaymentResponse>(), "No history found.");
			}

			var dtoList = _mapper.Map<List<GetPaymentResponse>>(paymentList);
			return BaseResponse<List<GetPaymentResponse>>.Ok(dtoList, "Retrieve history successfully.");
		}

		public async Task<BaseResponse<GetPaymentDetailResponse>> GetUserPaymentByIdAsync(Guid paymentId, Guid userId)
		{
			if (paymentId == Guid.Empty)
				return BaseResponse<GetPaymentDetailResponse>.Fail("Invalid payment id.", ResponseErrorType.BadRequest);
			if (userId == Guid.Empty)
				return BaseResponse<GetPaymentDetailResponse>.Fail("Invalid user id.", ResponseErrorType.BadRequest);

			var payment = await _paymentRepository.GetByIdWithSubscriptionDetailsAsync(paymentId);

			if (payment == null)
			{
				return BaseResponse<GetPaymentDetailResponse>.Fail("Payment record not found.", ResponseErrorType.NotFound);
			}

			if (payment.Subscription == null || payment.Subscription.UserId != userId)
			{
				return BaseResponse<GetPaymentDetailResponse>.Fail("Payment record not found.", ResponseErrorType.NotFound);
			}

			var payload = _mapper.Map<GetPaymentDetailResponse>(payment);
			return BaseResponse<GetPaymentDetailResponse>.Ok(payload);
		}

		public async Task<BaseResponse<VnPayReturnResponse>> GetVnPayReturnResponseAsync(IQueryCollection queryParameters)
		{
			var vnPayResponse = _vnPayService.GetPaymentResponseAsync(queryParameters);
			if (vnPayResponse == null || vnPayResponse.PaymentId == Guid.Empty)
			{
				return BaseResponse<VnPayReturnResponse>.Fail("Invalid VNPay response.", ResponseErrorType.BadRequest);
			}

			if (vnPayResponse.IsSuccess == false)
			{
				return BaseResponse<VnPayReturnResponse>.Fail("VNPay payment failed: " + vnPayResponse.Message, ResponseErrorType.BadRequest);
			}

			var payment = await _unitOfWork.Payments.GetByIdAsync(vnPayResponse.PaymentId);
			if (payment == null)
			{
				return BaseResponse<VnPayReturnResponse>.Fail("Payment record not found.", ResponseErrorType.NotFound);
			}

			var subscriptionId = payment.SubscriptionId;

			var payload = new VnPayReturnResponse
			{
				PaymentId = payment.Id,
				SubscriptionId = subscriptionId
			};

			return BaseResponse<VnPayReturnResponse>.Ok(payload);
		}

		public async Task<BaseResponse<bool>> ProcessVnPayPaymentCallbackAsync(IQueryCollection queryParameters)
		{
			var vnPayResponse = _vnPayService.GetPaymentResponseAsync(queryParameters);
			if (vnPayResponse == null || vnPayResponse.PaymentId == Guid.Empty)
			{
				return BaseResponse<bool>.Fail("VNPay payment failed", ResponseErrorType.BadRequest);
			}

			// check payment 
			var payment = await _unitOfWork.Payments.GetByIdAsync(vnPayResponse.PaymentId);
			if (payment == null)
			{
				return BaseResponse<bool>.Fail("Payment record not found.", ResponseErrorType.NotFound);
			}
			if (payment.Status != PaymentStatus.Pending)
			{
				return BaseResponse<bool>.Ok(true, "Payment already processed.");
			}
			if (payment.AmountPaid != vnPayResponse.Amount)
			{
				return BaseResponse<bool>.Fail("Payment amount mismatch.", ResponseErrorType.BadRequest);
			}

			// check subscription
			var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(payment.SubscriptionId);
			if (subscription == null)
			{
				return BaseResponse<bool>.Fail("Subscription record not found.", ResponseErrorType.NotFound);
			}

			// update payment & subscription status
			await _unitOfWork.BeginTransactionAsync();
			try
			{
				payment.Status = (vnPayResponse.ResponseCode == "00") ? PaymentStatus.Success : PaymentStatus.Failed;
				payment.VnpResponseCode = vnPayResponse.ResponseCode;
				payment.GatewayTransactionId = vnPayResponse.TransactionNo;
				payment.Description = vnPayResponse.PaymentInfo;

				if (payment.Status == PaymentStatus.Success)
				{
					subscription.Status = SubscriptionStatus.Active;
				}

				_unitOfWork.Payments.Update(payment);
				_unitOfWork.Subscriptions.Update(subscription);

				await _unitOfWork.CommitTransactionAsync();
				return BaseResponse<bool>.Ok(true, "Payment processed successfully.");
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync();
				return BaseResponse<bool>.Fail("Error processing payment callback: " + ex.Message, ResponseErrorType.InternalError);
			}
		}
	}
}
