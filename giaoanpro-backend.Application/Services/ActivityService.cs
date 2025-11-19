using giaoanpro_backend.Application.DTOs.Requests.Activities;
using giaoanpro_backend.Application.DTOs.Responses.Activities;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Application.Services
{
	public class ActivityService : IActivityService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ActivityService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<PagedResult<ActivityResponse>>> GetActivitiesAsync(GetActivitiesQuery query)
		{
			try
			{
				var (activities, totalCount) = await _unitOfWork.Activities.GetPagedAsync(
					filter: a =>
						(!query.LessonPlanId.HasValue || a.LessonPlanId == query.LessonPlanId.Value) &&
						(!query.ParentId.HasValue || a.ParentId == query.ParentId.Value),
					include: q => q
						.Include(a => a.LessonPlan)
						.Include(a => a.Children),
					orderBy: q => q.OrderBy(a => a.CreatedAt),
					pageNumber: query.PageNumber,
					pageSize: query.PageSize,
					asNoTracking: true
				);

				var activityResponses = activities.Select(a => new ActivityResponse
				{
					Id = a.Id,
					LessonPlanId = a.LessonPlanId,
					LessonPlanTitle = a.LessonPlan.Title,
					ParentId = a.ParentId,
					Type = a.Type,
					Title = a.Title,
					Objective = a.Objective,
					Content = a.Content,
					Product = a.Product,
					Implementation = a.Implementation,
					ChildrenCount = a.Children.Count,
					CreatedAt = a.CreatedAt,
					UpdatedAt = a.UpdatedAt
				}).ToList();

				var pagedResult = new PagedResult<ActivityResponse>(activityResponses, query.PageNumber, query.PageSize, totalCount);

				return BaseResponse<PagedResult<ActivityResponse>>.Ok(pagedResult);
			}
			catch (Exception ex)
			{
				return BaseResponse<PagedResult<ActivityResponse>>.Fail($"Error retrieving activities: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<ActivityResponse>> GetActivityByIdAsync(Guid id)
		{
			try
			{
				var activity = await _unitOfWork.Activities.GetByIdWithChildrenAsync(id);

				if (activity == null)
				{
					return BaseResponse<ActivityResponse>.Fail("Activity not found", ResponseErrorType.NotFound);
				}

				var response = new ActivityResponse
				{
					Id = activity.Id,
					LessonPlanId = activity.LessonPlanId,
					LessonPlanTitle = activity.LessonPlan.Title,
					ParentId = activity.ParentId,
					Type = activity.Type,
					Title = activity.Title,
					Objective = activity.Objective,
					Content = activity.Content,
					Product = activity.Product,
					Implementation = activity.Implementation,
					ChildrenCount = activity.Children.Count,
					CreatedAt = activity.CreatedAt,
					UpdatedAt = activity.UpdatedAt
				};

				return BaseResponse<ActivityResponse>.Ok(response);
			}
			catch (Exception ex)
			{
				return BaseResponse<ActivityResponse>.Fail($"Error retrieving activity: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<ActivityResponse>> CreateActivityAsync(CreateActivityRequest request)
		{
			try
			{
				// Check if lesson plan exists
				var lessonPlan = await _unitOfWork.LessonPlans.GetByIdAsync(request.LessonPlanId);
				if (lessonPlan == null)
				{
					return BaseResponse<ActivityResponse>.Fail("Lesson plan not found", ResponseErrorType.NotFound);
				}

				// Check if parent activity exists (if ParentId is provided)
				if (request.ParentId.HasValue)
				{
					var parentActivity = await _unitOfWork.Activities.GetByIdAsync(request.ParentId.Value);
					if (parentActivity == null)
					{
						return BaseResponse<ActivityResponse>.Fail("Parent activity not found", ResponseErrorType.NotFound);
					}

					if (parentActivity.LessonPlanId != request.LessonPlanId)
					{
						return BaseResponse<ActivityResponse>.Fail("Parent activity must belong to the same lesson plan", ResponseErrorType.BadRequest);
					}
				}

				var activity = new Activity
				{
					Id = Guid.NewGuid(),
					LessonPlanId = request.LessonPlanId,
					ParentId = request.ParentId,
					Type = request.Type,
					Title = request.Title,
					Objective = request.Objective,
					Content = request.Content,
					Product = request.Product,
					Implementation = request.Implementation
				};

				await _unitOfWork.Activities.AddAsync(activity);
				await _unitOfWork.SaveChangesAsync();

				// Reload with navigation properties
				activity = await _unitOfWork.Activities.GetByIdWithChildrenAsync(activity.Id);

				var response = new ActivityResponse
				{
					Id = activity!.Id,
					LessonPlanId = activity.LessonPlanId,
					LessonPlanTitle = activity.LessonPlan.Title,
					ParentId = activity.ParentId,
					Type = activity.Type,
					Title = activity.Title,
					Objective = activity.Objective,
					Content = activity.Content,
					Product = activity.Product,
					Implementation = activity.Implementation,
					ChildrenCount = activity.Children.Count,
					CreatedAt = activity.CreatedAt,
					UpdatedAt = activity.UpdatedAt
				};

				return BaseResponse<ActivityResponse>.Ok(response, "Activity created successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<ActivityResponse>.Fail($"Error creating activity: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<ActivityResponse>> UpdateActivityAsync(Guid id, UpdateActivityRequest request)
		{
			try
			{
				var activity = await _unitOfWork.Activities.GetByIdAsync(id);

				if (activity == null)
				{
					return BaseResponse<ActivityResponse>.Fail("Activity not found", ResponseErrorType.NotFound);
				}

				// Check if lesson plan exists
				var lessonPlan = await _unitOfWork.LessonPlans.GetByIdAsync(request.LessonPlanId);
				if (lessonPlan == null)
				{
					return BaseResponse<ActivityResponse>.Fail("Lesson plan not found", ResponseErrorType.NotFound);
				}

				// Check if parent activity exists (if ParentId is provided)
				if (request.ParentId.HasValue)
				{
					var parentActivity = await _unitOfWork.Activities.GetByIdAsync(request.ParentId.Value);
					if (parentActivity == null)
					{
						return BaseResponse<ActivityResponse>.Fail("Parent activity not found", ResponseErrorType.NotFound);
					}

					if (parentActivity.LessonPlanId != request.LessonPlanId)
					{
						return BaseResponse<ActivityResponse>.Fail("Parent activity must belong to the same lesson plan", ResponseErrorType.BadRequest);
					}

					// Prevent circular reference
					if (request.ParentId.Value == id)
					{
						return BaseResponse<ActivityResponse>.Fail("Activity cannot be its own parent", ResponseErrorType.BadRequest);
					}
				}

				activity.LessonPlanId = request.LessonPlanId;
				activity.ParentId = request.ParentId;
				activity.Type = request.Type;
				activity.Title = request.Title;
				activity.Objective = request.Objective;
				activity.Content = request.Content;
				activity.Product = request.Product;
				activity.Implementation = request.Implementation;

				_unitOfWork.Activities.Update(activity);
				await _unitOfWork.SaveChangesAsync();

				// Reload with navigation properties
				activity = await _unitOfWork.Activities.GetByIdWithChildrenAsync(id);

				var response = new ActivityResponse
				{
					Id = activity!.Id,
					LessonPlanId = activity.LessonPlanId,
					LessonPlanTitle = activity.LessonPlan.Title,
					ParentId = activity.ParentId,
					Type = activity.Type,
					Title = activity.Title,
					Objective = activity.Objective,
					Content = activity.Content,
					Product = activity.Product,
					Implementation = activity.Implementation,
					ChildrenCount = activity.Children.Count,
					CreatedAt = activity.CreatedAt,
					UpdatedAt = activity.UpdatedAt
				};

				return BaseResponse<ActivityResponse>.Ok(response, "Activity updated successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<ActivityResponse>.Fail($"Error updating activity: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse> DeleteActivityAsync(Guid id)
		{
			try
			{
				var activity = await _unitOfWork.Activities.GetByIdAsync(id);

				if (activity == null)
				{
					return BaseResponse.Fail("Activity not found", ResponseErrorType.NotFound);
				}

				_unitOfWork.Activities.Remove(activity);
				await _unitOfWork.SaveChangesAsync();

				return BaseResponse.Ok("Activity deleted successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse.Fail($"Error deleting activity: {ex.Message}", ResponseErrorType.InternalError);
			}
		}
	}
}
