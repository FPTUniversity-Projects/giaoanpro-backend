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

			_repository.Remove(existingPlan);
			var result = await _repository.SaveChangesAsync();
			return result
				? BaseResponse<string>.Ok(null!, "Subscription plan deleted successfully.")
				: BaseResponse<string>.Fail("Failed to delete subscription plan.", ResponseErrorType.InternalError);
		}

		public async Task<BaseResponse<List<GetSubscriptionPlanResponse>>> GetAllSubscriptionPlansAsync()
		{
			var subscriptionPlans = await _repository.GetAllPlansAsync();
			var response = _mapper.Map<List<GetSubscriptionPlanResponse>>(subscriptionPlans);
			if (!subscriptionPlans.Any())
			{
				return BaseResponse<List<GetSubscriptionPlanResponse>>.Ok(response, "No subscription plans found.");
			}
			return BaseResponse<List<GetSubscriptionPlanResponse>>.Ok(response, "Subscription plans retrieved successfully.");
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
	}
}
