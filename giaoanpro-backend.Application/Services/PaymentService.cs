using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace giaoanpro_backend.Application.Services
{
	public class PaymentService : IPaymentService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IVnPayService _vnPayService;

		public PaymentService(IUnitOfWork unitOfWork, IVnPayService vnPayService)
		{
			_unitOfWork = unitOfWork;
			_vnPayService = vnPayService;
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
