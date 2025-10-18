using giaoanpro_backend.Application.DTOs.Requests.Subscriptions;
using giaoanpro_backend.Application.DTOs.Requests.VnPays;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Subscriptions;
using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;
using giaoanpro_backend.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace giaoanpro_backend.Application.Services
{
	public class SubscriptionService : ISubscriptionService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IVnPayService _vnPayService;

		public SubscriptionService(IUnitOfWork unitOfWork, IVnPayService vnPayService)
		{
			_unitOfWork = unitOfWork;
			_vnPayService = vnPayService;
		}

		public async Task<BaseResponse<SubscriptionCheckoutResponse>> CreateSubscriptionCheckoutSessionAsync(SubscriptionCheckoutRequest request, HttpContext httpContext)
		{
			var plan = await _unitOfWork.Repository<SubscriptionPlan>().GetByConditionAsync(p => p.Id == request.PlanId && p.IsActive);
			if (plan == null)
			{
				return BaseResponse<SubscriptionCheckoutResponse>.Fail("Subscription plan not found.");
			}

			var alreadyHasActiveSubscription = await _unitOfWork.Repository<Subscription>()
				.AnyAsync(s => s.UserId == request.UserId && s.Status == Domain.Enums.SubscriptionStatus.Active);
			if (alreadyHasActiveSubscription)
			{
				return BaseResponse<SubscriptionCheckoutResponse>.Fail("User already has an active subscription.");
			}

			await _unitOfWork.BeginTransactionAsync();
			try
			{
				var now = DateTime.UtcNow;
				var subscription = new Subscription
				{
					Id = Guid.NewGuid(),
					UserId = request.UserId,
					PlanId = request.PlanId,
					StartDate = now,
					EndDate = now.AddMonths(plan.DurationInDays),
					Status = Domain.Enums.SubscriptionStatus.Inactive
				};

				var payment = new Payment
				{
					Id = Guid.NewGuid(),
					SubscriptionId = subscription.Id,
					AmountPaid = plan.Price,
					PaymentDate = now,
					Status = Domain.Enums.PaymentStatus.Pending,
					PaymentMethod = "VNPAY"
				};

				// Persist entities via UnitOfWork's generic repositories
				await _unitOfWork.Repository<Subscription>().AddAsync(subscription);
				await _unitOfWork.Repository<Payment>().AddAsync(payment);

				var vnpayRequest = new VnPaymentRequest
				{
					Amount = (int)Math.Round(plan.Price),
					SubscriptionId = subscription.Id,
					PaymentId = payment.Id
				};

				var result = await _vnPayService.CreatePaymentUrl(httpContext, vnpayRequest);
				if (result == null)
				{
					await _unitOfWork.RollbackTransactionAsync();
					return BaseResponse<SubscriptionCheckoutResponse>.Fail("Failed to create VNPay payment URL.");
				}

				// Commit will call SaveChangesAsync internally in UnitOfWork implementation
				await _unitOfWork.CommitTransactionAsync();

				var response = new SubscriptionCheckoutResponse
				{
					PaymentUrl = result.PaymentUrl,
					SubscriptionId = subscription.Id
				};

				return BaseResponse<SubscriptionCheckoutResponse>.Ok(response, "VNPay payment URL created successfully.");
			}
			catch (Exception ex)
			{
				await _unitOfWork.RollbackTransactionAsync();
				return BaseResponse<SubscriptionCheckoutResponse>.Fail("Error creating subscription checkout session: " + ex.Message);
			}
		}
	}
}
