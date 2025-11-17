using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.SubscriptionPlans;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.SubscriptionPlans;
using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;
using MapsterMapper;

namespace giaoanpro_backend.Application.Services
{
	public class SubscriptionPlanService : ISubscriptionPlanService
	{
		private readonly ISubscriptionPlanRepository _repository;
		private readonly IMapper _mapper;
		private readonly IValidator<CreateSubscriptionPlanRequest> _createValidator;
		private readonly IValidator<UpdateSubscriptionPlanRequest> _updateValidator;

		public SubscriptionPlanService(ISubscriptionPlanRepository repository, IMapper mapper, IValidator<CreateSubscriptionPlanRequest> createValidator, IValidator<UpdateSubscriptionPlanRequest> updateValidator)
		{
			_repository = repository;
			_mapper = mapper;
			_createValidator = createValidator;
			_updateValidator = updateValidator;
		}

		public async Task<BaseResponse<string>> CreateSubscriptionPlanAsync(CreateSubscriptionPlanRequest request)
		{
			var validationResult = await _createValidator.ValidateAsync(request);
			if (!validationResult.IsValid)
			{
				var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				return BaseResponse<string>.Fail("Validation failed.", ResponseErrorType.BadRequest, errors);
			}
			var subscriptionPlan = _mapper.Map<SubscriptionPlan>(request);
			await _repository.AddAsync(subscriptionPlan);
			var result = await _repository.SaveChangesAsync();
			return result
				? BaseResponse<string>.Ok(subscriptionPlan.Id.ToString(), "Subscription plan created successfully.")
				: BaseResponse<string>.Fail("Failed to create subscription plan.", ResponseErrorType.InternalError);
		}

		public async Task<BaseResponse<string>> DeleteSubscriptionPlanAsync(Guid id)
		{
			var existingPlan = await _repository.GetPlanByIdAsync(id);
			if (existingPlan == null)
			{
				return BaseResponse<string>.Fail("Subscription plan not found.", ResponseErrorType.NotFound);
			}
			var hasSubscriptions = await _repository.HasSubscriptionsAsync(id);
			if (hasSubscriptions)
			{
				return BaseResponse<string>.Fail("Cannot delete subscription plan has subscriptions.", ResponseErrorType.BadRequest);
			}

			_repository.Remove(existingPlan);
			var result = await _repository.SaveChangesAsync();
			return result
				? BaseResponse<string>.Ok(null!, "Subscription plan deleted successfully.")
				: BaseResponse<string>.Fail("Failed to delete subscription plan.", ResponseErrorType.InternalError);
		}

		public async Task<BaseResponse<PagedResult<GetSubscriptionPlanResponse>>> GetSubscriptionPlansAsync(GetSubscriptionPlansQuery query)
		{
			// Determine paging values with safe defaults
			int pageNumber = query.PageNumber > 0 ? query.PageNumber : 1;
			int pageSize = query.PageSize > 0 ? Math.Min(query.PageSize, 50) : 10;

			// Determine sort direction
			bool descending = false;
			if (!string.IsNullOrWhiteSpace(query.SortOrder))
			{
				descending = string.Equals(query.SortOrder, "desc", StringComparison.OrdinalIgnoreCase);
			}
			// some callers may populate IsDescending helper
			descending = descending || query.IsDescending;

			var (items, totalCount) = await _repository.GetSubscriptionPlansAsync(
				query.Search,
				query.MinPrice,
				query.MaxPrice,
				query.MinDurationInDays,
				query.MaxDurationInDays,
				query.MinLessons,
				query.MinPromptsPerDay,
				query.IsActive,
				query.SortBy,
				descending,
				pageNumber,
				pageSize);

			var mapped = _mapper.Map<List<GetSubscriptionPlanResponse>>(items);

			var paged = new PagedResult<GetSubscriptionPlanResponse>(mapped, pageNumber, pageSize, totalCount);

			// Optionally you could return paging metadata; current contract returns list only.
			if (mapped.Count == 0)
			{
				return BaseResponse<PagedResult<GetSubscriptionPlanResponse>>.Ok(paged, "No subscription plans found.");
			}

			return BaseResponse<PagedResult<GetSubscriptionPlanResponse>>.Ok(paged, "Subscription plans retrieved successfully.");
		}

		public async Task<BaseResponse<GetSubscriptionPlanResponse>> GetSubscriptionPlanByIdAsync(Guid id)
		{
			var existingPlan = await _repository.GetPlanByIdAsync(id);
			if (existingPlan == null)
			{
				return BaseResponse<GetSubscriptionPlanResponse>.Fail("Subscription plan not found.", ResponseErrorType.NotFound);
			}
			var response = _mapper.Map<GetSubscriptionPlanResponse>(existingPlan);
			return BaseResponse<GetSubscriptionPlanResponse>.Ok(response, "Subscription plan retrieved successfully.");
		}

		public async Task<BaseResponse<string>> UpdateSubscriptionPlanAsync(Guid id, UpdateSubscriptionPlanRequest request)
		{
			var validationResult = await _updateValidator.ValidateAsync(request);
			if (!validationResult.IsValid)
			{
				var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
				return BaseResponse<string>.Fail("Validation failed.", ResponseErrorType.BadRequest, errors);
			}
			var existingPlan = await _repository.GetPlanByIdAsync(id);
			if (existingPlan == null)
			{
				return BaseResponse<string>.Fail("Subscription plan not found.", ResponseErrorType.NotFound);
			}
			_mapper.Map(request, existingPlan);
			_repository.Update(existingPlan);
			var result = await _repository.SaveChangesAsync();
			return result
				? BaseResponse<string>.Ok(null!, "Subscription plan updated successfully.")
				: BaseResponse<string>.Fail("Failed to update subscription plan.", ResponseErrorType.InternalError);
		}

		public async Task<BaseResponse<List<SubscriptionPlanLookupResponse>>> GetSubscriptionPlanLookupsAsync()
		{
			var plans = await _repository.GetSubscriptionPlansAsync(onlyActive: true);
			return BaseResponse<List<SubscriptionPlanLookupResponse>>.Ok(
				_mapper.Map<List<SubscriptionPlanLookupResponse>>(plans),
				"Subscription plan lookups retrieved successfully.");
		}

		public async Task<BaseResponse<List<SubscriptionPlanLookupResponse>>> GetSubscriptionPlanAdminLookupsAsync(bool isActiveOnly)
		{
			var plans = await _repository.GetSubscriptionPlansAsync(onlyActive: isActiveOnly);
			return BaseResponse<List<SubscriptionPlanLookupResponse>>.Ok(
				_mapper.Map<List<SubscriptionPlanLookupResponse>>(plans),
				"Subscription plan admin lookups retrieved successfully.");
		}
	}
}
