using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Subscriptions;
using giaoanpro_backend.Application.DTOs.Requests.VnPays;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Subscriptions;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace giaoanpro_backend.Application.Services
{
	public class SubscriptionService : ISubscriptionService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IVnPayService _vnPayService;
		private readonly IValidator<CreateSubscriptionRequest> _createSubscriptionValidator;
		private readonly IValidator<UpdateSubscriptionStatusRequest> _updateSubscriptionStatusValidator;
		private readonly IMapper _mapper;

		public SubscriptionService(IUnitOfWork unitOfWork, IVnPayService vnPayService, IMapper mapper, IValidator<CreateSubscriptionRequest> createSubscriptionValidator, IValidator<UpdateSubscriptionStatusRequest> updateSubscriptionStatusValidator)
		{
			_unitOfWork = unitOfWork;
			_vnPayService = vnPayService;
			_mapper = mapper;
			_createSubscriptionValidator = createSubscriptionValidator;
			_updateSubscriptionStatusValidator = updateSubscriptionStatusValidator;
		}

		public async Task<BaseResponse<string>> CancelSubscriptionAsync(Guid subscriptionId, Guid userId)
		{
			var subscription = await _unitOfWork.Subscriptions.GetByIdAndUserAsync(subscriptionId, userId);
			if (subscription == null)
			{
				return BaseResponse<string>.Fail("Subscription not found.", ResponseErrorType.NotFound);
			}
			if (subscription.Status != SubscriptionStatus.Active)
			{
				return BaseResponse<string>.Fail("Only active subscriptions can be cancelled.", ResponseErrorType.BadRequest);
			}
			subscription.Status = SubscriptionStatus.Canceled;
			_unitOfWork.Subscriptions.Update(subscription);
			var result = await _unitOfWork.SaveChangesAsync();
			return result
				? BaseResponse<string>.Ok("Subscription cancelled successfully.")
				: BaseResponse<string>.Fail("Failed to cancel subscription.", ResponseErrorType.InternalError);
		}

		public async Task<BaseResponse<SubscriptionCheckoutResponse>> CreateSubscriptionCheckoutSessionAsync(Guid userId, SubscriptionCheckoutRequest request, HttpContext httpContext)
		{
			var preparationResult = await PrepareSubscriptionForCheckoutAsync(userId, request);
			if (!preparationResult.Success)
			{
				return BaseResponse<SubscriptionCheckoutResponse>.Fail(preparationResult.Message, preparationResult.ErrorType);
			}

			var (subscription, plan) = preparationResult.Payload;

			await _unitOfWork.BeginTransactionAsync();
			try
			{
				// If the plan is free (price == 0) activate subscription immediately without creating a payment or calling VNPay.
				if (plan.Price <= 0m)
				{
					subscription.Status = SubscriptionStatus.Active;

					_unitOfWork.Subscriptions.Update(subscription);

					var saved = await _unitOfWork.SaveChangesAsync();
					if (!saved)
					{
						await _unitOfWork.RollbackTransactionAsync();
						return BaseResponse<SubscriptionCheckoutResponse>.Fail("Failed to activate free subscription.", ResponseErrorType.InternalError);
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
				await _unitOfWork.Payments.AddAsync(payment);

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
					return BaseResponse<SubscriptionCheckoutResponse>.Fail("Failed to create VNPay payment URL.", ResponseErrorType.InternalError);
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
				return BaseResponse<SubscriptionCheckoutResponse>.Fail("An error occurred while creating the checkout session.", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<GetSubscriptionResponse>> GetCurrentAccessSubscriptionByUserIdAsync(Guid userId)
		{
			var subscription = await _unitOfWork.Subscriptions.GetCurrentAccessByUserAsync(userId);
			if (subscription == null)
			{
				return BaseResponse<GetSubscriptionResponse>.Fail("No access subscription found for the user.", ResponseErrorType.NotFound);
			}
			var response = _mapper.Map<GetSubscriptionResponse>(subscription);
			return BaseResponse<GetSubscriptionResponse>.Ok(response, "Current access subscription retrieved successfully.");
		}

		public async Task<BaseResponse<GetSubscriptionDetailResponse>> GetUserSubscriptionByIdAsync(Guid subscriptionId, Guid userId)
		{
			var subscription = await _unitOfWork.Subscriptions.GetByIdAndUserAsync(subscriptionId, userId, includePlan: true, includePayments: true);
			if (subscription == null)
			{
				return BaseResponse<GetSubscriptionDetailResponse>.Fail("Subscription not found for the user.", ResponseErrorType.NotFound);
			}
			var response = _mapper.Map<GetSubscriptionDetailResponse>(subscription);
			return BaseResponse<GetSubscriptionDetailResponse>.Ok(response, "Subscription retrieved successfully.");
		}

		public async Task<BaseResponse<PagedResult<GetHistorySubscriptionResponse>>> GetSubscriptionHistoryByUserIdAsync(Guid userId, GetMySubscriptionHistoryQuery query)
		{
			if (userId == Guid.Empty)
			{
				return BaseResponse<PagedResult<GetHistorySubscriptionResponse>>.Fail("Invalid user id.", ResponseErrorType.BadRequest);
			}

			int pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
			int pageSize = query.PageSize > 0 ? Math.Min(query.PageSize, 50) : 10;

			bool descending = false;
			if (!string.IsNullOrWhiteSpace(query.SortOrder))
			{
				descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
			}
			descending = descending || query.IsDescending;

			var (items, totalCount) = await _unitOfWork.Subscriptions.GetSubscriptionsAsync(
				search: null,
				userId: userId,
				planId: query.PlanId,
				status: query.Status,
				expiresBefore: null,
				expiresAfter: null,
				minPromptsUsed: null,
				minLessonsCreated: null,
				sortBy: query.SortBy,
				isDescending: descending,
				pageNumber: pageNumber,
				pageSize: pageSize
			);

			var mapped = _mapper.Map<List<GetHistorySubscriptionResponse>>(items);
			var paged = new PagedResult<GetHistorySubscriptionResponse>(mapped, pageNumber, pageSize, totalCount);

			if (mapped.Count == 0)
			{
				return BaseResponse<PagedResult<GetHistorySubscriptionResponse>>.Ok(paged, "No subscription history found for the user.");
			}

			return BaseResponse<PagedResult<GetHistorySubscriptionResponse>>.Ok(paged, "Subscription history retrieved successfully.");
		}

		private async Task<BaseResponse<(Subscription, SubscriptionPlan)>> PrepareSubscriptionForCheckoutAsync(Guid userId, SubscriptionCheckoutRequest request)
		{
			Subscription? subscription;
			SubscriptionPlan? plan;

			if (request.SubscriptionId.HasValue) // Retry Flow
			{
				subscription = await _unitOfWork.Subscriptions.GetPendingRetryAsync(request.SubscriptionId.Value, userId);
				if (subscription == null)
				{
					return BaseResponse<(Subscription, SubscriptionPlan)>.Fail("Pending subscription not found for retry.", ResponseErrorType.NotFound);
				}

				plan = await _unitOfWork.SubscriptionPlans.GetPlanByIdAsync(subscription.PlanId);
				if (plan == null)
				{
					return BaseResponse<(Subscription, SubscriptionPlan)>.Fail("Associated plan not found.", ResponseErrorType.NotFound);
				}
			}
			else // New Subscription Flow
			{
				plan = await _unitOfWork.SubscriptionPlans.GetPlanByIdAsync(request.PlanId);
				if (plan == null || !plan.IsActive)
				{
					return BaseResponse<(Subscription, SubscriptionPlan)>.Fail("Subscription plan not found or is not active.", ResponseErrorType.NotFound);
				}

				var alreadyHasActiveSubscription = await _unitOfWork.Subscriptions.UserHasActiveSubscriptionAsync(userId);
				if (alreadyHasActiveSubscription)
				{
					return BaseResponse<(Subscription, SubscriptionPlan)>.Fail("User already has an active subscription.", ResponseErrorType.Conflict);
				}

				var now = DateTime.UtcNow;
				subscription = new Subscription
				{
					Id = Guid.NewGuid(),
					UserId = userId,
					PlanId = request.PlanId,
					StartDate = now,
					EndDate = now.AddDays(plan.DurationInDays),
					Status = SubscriptionStatus.Inactive,
					CurrentLessonPlansCreated = 0,
					CurrentPromptsUsed = 0,
					LastPromptResetDate = null
				};

				await _unitOfWork.Subscriptions.AddAsync(subscription);
			}

			return BaseResponse<(Subscription, SubscriptionPlan)>.Ok((subscription, plan));
		}

		public async Task<BaseResponse<PagedResult<GetHistorySubscriptionResponse>>> GetSubscriptionsAsync(GetSubscriptionsQuery query)
		{
			int pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
			int pageSize = query.PageSize > 0 ? Math.Min(query.PageSize, 50) : 10;

			bool descending = false;
			if (!string.IsNullOrWhiteSpace(query.SortOrder))
			{
				descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
			}
			descending = descending || query.IsDescending;

			var (items, totalCount) = await _unitOfWork.Subscriptions.GetSubscriptionsAsync(
				search: query.Search,
				userId: query.UserId,
				planId: query.PlanId,
				status: query.Status,
				expiresBefore: query.ExpiresBefore,
				expiresAfter: query.ExpiresAfter,
				minPromptsUsed: query.MinPromptsUsed,
				minLessonsCreated: query.MinLessonsCreated,
				sortBy: query.SortBy,
				isDescending: descending,
				pageNumber: pageNumber,
				pageSize: pageSize
			);

			var mapped = _mapper.Map<List<GetHistorySubscriptionResponse>>(items);
			var paged = new PagedResult<GetHistorySubscriptionResponse>(mapped, pageNumber, pageSize, totalCount);

			if (mapped.Count == 0)
			{
				return BaseResponse<PagedResult<GetHistorySubscriptionResponse>>.Ok(paged, "No subscriptions found.");
			}

			return BaseResponse<PagedResult<GetHistorySubscriptionResponse>>.Ok(paged, "Subscriptions retrieved successfully.");
		}

		public async Task<BaseResponse<string>> CreateSubscriptionAsync(CreateSubscriptionRequest request)
		{
			var validationResult = await _createSubscriptionValidator.ValidateAsync(request);
			if (!validationResult.IsValid)
			{
				var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				return BaseResponse<string>.Fail("Validation failed", ResponseErrorType.BadRequest, errors);
			}

			var plan = await _unitOfWork.SubscriptionPlans.GetPlanByIdAsync(request.PlanId);
			if (plan == null || !plan.IsActive)
			{
				return BaseResponse<string>.Fail("Subscription plan not found or is not active.", ResponseErrorType.NotFound);
			}
			var userHasActiveSubscription = await _unitOfWork.Subscriptions.UserHasActiveSubscriptionAsync(request.UserId);
			if (userHasActiveSubscription && request.Status == SubscriptionStatus.Active)
			{
				return BaseResponse<string>.Fail("User already has an active subscription.", ResponseErrorType.Conflict);
			}
			var now = DateTime.UtcNow;
			var subscription = new Subscription
			{
				Id = Guid.NewGuid(),
				UserId = request.UserId,
				PlanId = request.PlanId,
				StartDate = now,
				EndDate = now.AddDays(plan.DurationInDays),
				Status = request.Status,
				CurrentLessonPlansCreated = 0,
				CurrentPromptsUsed = 0,
				LastPromptResetDate = null
			};
			await _unitOfWork.Subscriptions.AddAsync(subscription);
			var result = await _unitOfWork.SaveChangesAsync();
			return result
				? BaseResponse<string>.Ok("Subscription created successfully.")
				: BaseResponse<string>.Fail("Failed to create subscription.", ResponseErrorType.InternalError);
		}

		public async Task<BaseResponse<string>> UpdateSubscriptionStatusAsync(Guid subscriptionId, UpdateSubscriptionStatusRequest request)
		{
			var validationResult = await _updateSubscriptionStatusValidator.ValidateAsync(request);
			if (!validationResult.IsValid)
			{
				var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				return BaseResponse<string>.Fail("Validation failed", ResponseErrorType.BadRequest, errors);
			}
			var subscription = await _unitOfWork.Subscriptions.GetByIdAsync(subscriptionId);
			if (subscription == null)
			{
				return BaseResponse<string>.Fail("Subscription not found.", ResponseErrorType.NotFound);
			}
			var activeSubscriptionExists = await _unitOfWork.Subscriptions.UserHasActiveSubscriptionAsync(subscription.UserId);
			if (request.Status == SubscriptionStatus.Active && activeSubscriptionExists && subscription.Status != SubscriptionStatus.Active)
			{
				return BaseResponse<string>.Fail("User already has an active subscription.", ResponseErrorType.Conflict);
			}
			subscription.Status = request.Status;
			_unitOfWork.Subscriptions.Update(subscription);
			var result = await _unitOfWork.SaveChangesAsync();
			return result
				? BaseResponse<string>.Ok("Subscription status updated successfully.")
				: BaseResponse<string>.Fail("Failed to update subscription status.", ResponseErrorType.InternalError);
		}
	}
}
