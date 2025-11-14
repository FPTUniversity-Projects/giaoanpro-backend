using giaoanpro_backend.Application.DTOs.Requests.Subscriptions;
using giaoanpro_backend.Application.DTOs.Requests.VnPays;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Subscriptions;
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
	public class SubscriptionService : ISubscriptionService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IVnPayService _vnPayService;
		private readonly IMapper _mapper;

		public SubscriptionService(IUnitOfWork unitOfWork, IVnPayService vnPayService, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_vnPayService = vnPayService;
			_mapper = mapper;
		}

		public async Task<BaseResponse<string>> CancelSubscriptionAsync(Guid subscriptionId, Guid userId)
		{
			var subscription = await _unitOfWork.Repository<Subscription>().
				GetByConditionAsync(s => s.Id == subscriptionId && s.UserId == userId);
			if (subscription == null)
			{
				return BaseResponse<string>.Fail("Subscription not found.");
			}
			if (subscription.Status != SubscriptionStatus.Active)
			{
				return BaseResponse<string>.Fail("Only active subscriptions can be cancelled.");
			}
			subscription.Status = SubscriptionStatus.Canceled;
			_unitOfWork.Repository<Subscription>().Update(subscription);
			var result = await _unitOfWork.SaveChangesAsync();
			return result
				? BaseResponse<string>.Ok("Subscription cancelled successfully.")
				: BaseResponse<string>.Fail("Failed to cancel subscription.");
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
				// If the plan is free (price == 0) activate subscription immediately without creating a payment or calling VNPay.
				if (plan.Price <= 0m)
				{
					subscription.Status = SubscriptionStatus.Active;

					_unitOfWork.Repository<Subscription>().Update(subscription);

					var saved = await _unitOfWork.SaveChangesAsync();
					if (!saved)
					{
						await _unitOfWork.RollbackTransactionAsync();
						return BaseResponse<SubscriptionCheckoutResponse>.Fail("Failed to activate free subscription.");
					}

					await _unitOfWork.CommitTransactionAsync();

					var freeResponse = new SubscriptionCheckoutResponse
					{
						PaymentUrl = string.Empty,
						SubscriptionId = subscription.Id
					};

					return BaseResponse<SubscriptionCheckoutResponse>.Ok(freeResponse, "Free subscription activated successfully.");
				}

				// Paid flow (existing behavior)
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

		public async Task<BaseResponse<GetSubscriptionResponse>> GetCurrentAccessSubscriptionByUserIdAsync(Guid userId)
		{
			var subscription = await _unitOfWork.Repository<Subscription>()
				.GetByConditionAsync(s => s.UserId == userId &&
					(s.Status == SubscriptionStatus.Active ||
						(s.Status == SubscriptionStatus.Canceled && s.EndDate >= DateTime.UtcNow)));
			if (subscription == null)
			{
				return BaseResponse<GetSubscriptionResponse>.Fail("No access subscription found for the user.");
			}
			var response = _mapper.Map<GetSubscriptionResponse>(subscription);
			return BaseResponse<GetSubscriptionResponse>.Ok(response, "Current access subscription retrieved successfully.");
		}

		public async Task<BaseResponse<GetSubscriptionDetailResponse>> GetUserSubscriptionByIdAsync(Guid subscriptionId, Guid userId)
		{
			var subscription = await _unitOfWork.Repository<Subscription>()
				.GetByConditionAsync(s => s.Id == subscriptionId && s.UserId == userId,
					include: s => s
						.Include(sub => sub.Plan)
						.Include(sub => sub.Payments));
			if (subscription == null)
			{
				return BaseResponse<GetSubscriptionDetailResponse>.Fail("Subscription not found for the user.");
			}
			var response = _mapper.Map<GetSubscriptionDetailResponse>(subscription);
			return BaseResponse<GetSubscriptionDetailResponse>.Ok(response, "Subscription retrieved successfully.");
		}

		public async Task<BaseResponse<List<GetHistorySubscriptionResponse>>> GetSubscriptionHistoryByUserIdAsync(Guid userId)
		{
			var subscriptions = await _unitOfWork.Repository<Subscription>()
				.GetAllAsync(s => s.UserId == userId);
			if (!subscriptions.Any())
			{
				return BaseResponse<List<GetHistorySubscriptionResponse>>.Ok(new List<GetHistorySubscriptionResponse>(), "No subscription history found for the user.");
			}
			var response = _mapper.Map<List<GetHistorySubscriptionResponse>>(subscriptions);
			return BaseResponse<List<GetHistorySubscriptionResponse>>.Ok(response, "Subscription history retrieved successfully.");
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
					Status = SubscriptionStatus.Inactive,
					CurrentLessonPlansCreated = 0,
					CurrentPromptsUsed = 0,
					LastPromptResetDate = null
				};

				await _unitOfWork.Repository<Subscription>().AddAsync(subscription);
			}

			return BaseResponse<(Subscription, SubscriptionPlan)>.Ok((subscription, plan));
		}
	}
}
