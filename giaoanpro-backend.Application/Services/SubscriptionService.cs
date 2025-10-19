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
			var preparationResult = await PrepareSubscriptionForCheckoutAsync(request);
			if (!preparationResult.Success)
			{
				return BaseResponse<SubscriptionCheckoutResponse>.Fail(preparationResult.Message);
			}

			var (subscription, plan) = preparationResult.Payload;

			await _unitOfWork.BeginTransactionAsync();
			try
			{
				var payment = new Payment
				{
					Id = Guid.NewGuid(),
					SubscriptionId = subscription.Id,
					AmountPaid = plan.Price,
					PaymentDate = DateTime.UtcNow,
					Status = PaymentStatus.Pending,
					PaymentMethod = "VNPAY"
				};
				await _unitOfWork.Repository<Payment>().AddAsync(payment);

				var vnpayRequest = new VnPaymentRequest
				{
					Amount = (int)Math.Round(plan.Price),
					SubscriptionId = subscription.Id,
					PaymentId = payment.Id
				};

				var result = await _vnPayService.CreatePaymentUrlAsync(httpContext, vnpayRequest);
				if (result == null)
				{
					await _unitOfWork.RollbackTransactionAsync();
					return BaseResponse<SubscriptionCheckoutResponse>.Fail("Failed to create VNPay payment URL.");
				}

				await _unitOfWork.CommitTransactionAsync();

				var response = new SubscriptionCheckoutResponse
				{
					PaymentUrl = result.PaymentUrl,
					SubscriptionId = subscription.Id
				};

				return BaseResponse<SubscriptionCheckoutResponse>.Ok(response, "VNPay payment URL created successfully.");
			}
			catch (Exception)
			{
				await _unitOfWork.RollbackTransactionAsync();
				return BaseResponse<SubscriptionCheckoutResponse>.Fail("An error occurred while creating the checkout session.");
			}
		}

		private async Task<BaseResponse<(Subscription, SubscriptionPlan)>> PrepareSubscriptionForCheckoutAsync(SubscriptionCheckoutRequest request)
		{
			Subscription? subscription;
			SubscriptionPlan? plan;

			if (request.SubscriptionId.HasValue) // Retry Flow
			{
				subscription = await _unitOfWork.Repository<Subscription>()
					.GetByConditionAsync(s => s.Id == request.SubscriptionId.Value &&
											  s.UserId == request.UserId &&
											  s.Status == SubscriptionStatus.Inactive);
				if (subscription == null)
				{
					return BaseResponse<(Subscription, SubscriptionPlan)>.Fail("Pending subscription not found for retry.");
				}

				plan = await _unitOfWork.Repository<SubscriptionPlan>().GetByIdAsync(subscription.PlanId);
				if (plan == null)
				{
					return BaseResponse<(Subscription, SubscriptionPlan)>.Fail("Associated plan not found.");
				}
			}
			else // New Subscription Flow
			{
				plan = await _unitOfWork.Repository<SubscriptionPlan>().GetByConditionAsync(p => p.Id == request.PlanId && p.IsActive);
				if (plan == null)
				{
					return BaseResponse<(Subscription, SubscriptionPlan)>.Fail("Subscription plan not found or is not active.");
				}

				var alreadyHasActiveSubscription = await _unitOfWork.Repository<Subscription>()
					.AnyAsync(s => s.UserId == request.UserId && s.Status == SubscriptionStatus.Active);
				if (alreadyHasActiveSubscription)
				{
					return BaseResponse<(Subscription, SubscriptionPlan)>.Fail("User already has an active subscription.");
				}

				var now = DateTime.UtcNow;
				subscription = new Subscription
				{
					Id = Guid.NewGuid(),
					UserId = request.UserId,
					PlanId = request.PlanId,
					StartDate = now,
					EndDate = now.AddDays(plan.DurationInDays),
					Status = SubscriptionStatus.Inactive
				};

				await _unitOfWork.Repository<Subscription>().AddAsync(subscription);
			}

			return BaseResponse<(Subscription, SubscriptionPlan)>.Ok((subscription, plan));
		}
	}
}
