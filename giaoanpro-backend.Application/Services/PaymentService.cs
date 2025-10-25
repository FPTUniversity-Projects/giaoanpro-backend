using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Payments;
using giaoanpro_backend.Application.DTOs.Responses.VnPays;
using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Application.Services
{
	public class PaymentService : IPaymentService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IVnPayService _vnPayService;
		private readonly IMapper _mapper;

		public PaymentService(IUnitOfWork unitOfWork, IVnPayService vnPayService, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_vnPayService = vnPayService;
			_mapper = mapper;
		}

		public async Task<BaseResponse<List<GetPaymentResponse>>> GetPaymentHistoryByUserIdAsync(Guid userId)
		{
			if (userId == Guid.Empty)
				return BaseResponse<List<GetPaymentResponse>>.Fail("Invalid user id.");

			var payments = await _unitOfWork.Repository<Payment>().GetAllAsync(
				filter: p => p.Subscription.UserId == userId,
				include: q => q
					.Include(p => p.Subscription)
						.ThenInclude(s => s.Plan),
				orderBy: q => q.OrderByDescending(p => p.PaymentDate),
				asNoTracking: true
			);

			var paymentList = payments?.ToList() ?? [];

			if (paymentList.Count == 0)
			{
				return BaseResponse<List<GetPaymentResponse>>.Ok([], "No history found.");
			}

			var dtoList = _mapper.Map<List<GetPaymentResponse>>(paymentList);
			return BaseResponse<List<GetPaymentResponse>>.Ok(dtoList, "Retrieve history successfully.");
		}

		public async Task<BaseResponse<GetPaymentDetailResponse>> GetUserPaymentByIdAsync(Guid paymentId, Guid userId)
		{
			if (paymentId == Guid.Empty)
				return BaseResponse<GetPaymentDetailResponse>.Fail("Invalid payment id.");
			if (userId == Guid.Empty)
				return BaseResponse<GetPaymentDetailResponse>.Fail("Invalid user id.");

			var payment = await _unitOfWork.Repository<Payment>().FirstOrDefaultAsync(
				p => p.Id == paymentId,
				include: q => q
					.Include(p => p.Subscription)
						.ThenInclude(s => s.User)
					.Include(p => p.Subscription)
						.ThenInclude(s => s.Plan),
				asNoTracking: true
			);

			if (payment == null)
			{
				return BaseResponse<GetPaymentDetailResponse>.Fail("Payment record not found.");
			}

			if (payment.Subscription == null || payment.Subscription.UserId != userId)
			{
				return BaseResponse<GetPaymentDetailResponse>.Fail("Payment record not found.");
			}

			var payload = _mapper.Map<GetPaymentDetailResponse>(payment);
			return BaseResponse<GetPaymentDetailResponse>.Ok(payload);
		}

		public async Task<BaseResponse<VnPayReturnResponse>> GetVnPayReturnResponseAsync(IQueryCollection queryParameters)
		{
			var vnPayResponse = await _vnPayService.GetPaymentResponseAsync(queryParameters);
			if (vnPayResponse == null || vnPayResponse.PaymentId == Guid.Empty)
			{
				return BaseResponse<VnPayReturnResponse>.Fail("Invalid VNPay response.");
			}

			if (vnPayResponse.IsSuccess == false)
			{
				return BaseResponse<VnPayReturnResponse>.Fail("VNPay payment failed: " + vnPayResponse.Message);
			}

			var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(vnPayResponse.PaymentId);
			if (payment == null)
			{
				return BaseResponse<VnPayReturnResponse>.Fail("Payment record not found.");
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
			var vnPayResponse = await _vnPayService.GetPaymentResponseAsync(queryParameters);
			if (vnPayResponse == null || vnPayResponse.PaymentId == Guid.Empty)
			{
				return BaseResponse<bool>.Fail("VNPay payment failed");
			}

			// check payment 
			var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(vnPayResponse.PaymentId);
			if (payment == null)
			{
				return BaseResponse<bool>.Fail("Payment record not found.");
			}
			if (payment.Status != PaymentStatus.Pending)
			{
				return BaseResponse<bool>.Ok(true, "Payment already processed.");
			}
			if (payment.AmountPaid != vnPayResponse.Amount)
			{
				return BaseResponse<bool>.Fail("Payment amount mismatch.");
			}

			// check subscription
			var subscription = await _unitOfWork.Repository<Subscription>().GetByIdAsync(payment.SubscriptionId);
			if (subscription == null)
			{
				return BaseResponse<bool>.Fail("Subscription record not found.");
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

				_unitOfWork.Repository<Payment>().Update(payment);
				_unitOfWork.Repository<Subscription>().Update(subscription);

				await _unitOfWork.CommitTransactionAsync();
				return BaseResponse<bool>.Ok(true, "Payment processed successfully.");
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync();
				return BaseResponse<bool>.Fail("Error processing payment callback: " + ex.Message);
			}
		}
	}
}
