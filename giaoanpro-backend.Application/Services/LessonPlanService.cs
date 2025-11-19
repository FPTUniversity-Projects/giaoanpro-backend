using giaoanpro_backend.Application.DTOs.Requests.LessonPlans;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.LessonPlans;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Application.Services
{
	public class LessonPlanService : ILessonPlanService
	{
		private readonly IUnitOfWork _unitOfWork;

		public LessonPlanService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<PagedResult<LessonPlanResponse>>> GetLessonPlansAsync(GetLessonPlansQuery query)
		{
			try
			{
				var (lessonPlans, totalCount) = await _unitOfWork.LessonPlans.GetPagedAsync(
					filter: lp =>
						(string.IsNullOrEmpty(query.Title) || lp.Title.ToLower().Contains(query.Title.ToLower())) &&
						(!query.SubjectId.HasValue || lp.SubjectId == query.SubjectId.Value) &&
						(!query.UserId.HasValue || lp.UserId == query.UserId.Value),
					include: q => q
						.Include(lp => lp.Subject)
						.Include(lp => lp.User)
						.Include(lp => lp.Activities),
					orderBy: q => q.OrderByDescending(lp => lp.CreatedAt),
					pageNumber: query.PageNumber,
					pageSize: query.PageSize,
					asNoTracking: true
				);

				var lessonPlanResponses = lessonPlans.Select(lp => new LessonPlanResponse
				{
					Id = lp.Id,
					UserId = lp.UserId,
					UserName = lp.User.FullName,
					SubjectId = lp.SubjectId,
					SubjectName = lp.Subject.Name,
					Title = lp.Title,
					Objective = lp.Objective,
					Note = lp.Note,
					ActivityCount = lp.Activities.Count,
					CreatedAt = lp.CreatedAt,
					UpdatedAt = lp.UpdatedAt
				}).ToList();

				var pagedResult = new PagedResult<LessonPlanResponse>(lessonPlanResponses, query.PageNumber, query.PageSize, totalCount);

				return BaseResponse<PagedResult<LessonPlanResponse>>.Ok(pagedResult);
			}
			catch (Exception ex)
			{
				return BaseResponse<PagedResult<LessonPlanResponse>>.Fail($"Error retrieving lesson plans: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<LessonPlanResponse>> GetLessonPlanByIdAsync(Guid id)
		{
			try
			{
				var lessonPlan = await _unitOfWork.LessonPlans.GetByIdWithActivitiesAsync(id);

				if (lessonPlan == null)
				{
					return BaseResponse<LessonPlanResponse>.Fail("Lesson plan not found", ResponseErrorType.NotFound);
				}

				var response = new LessonPlanResponse
				{
					Id = lessonPlan.Id,
					UserId = lessonPlan.UserId,
					UserName = lessonPlan.User.FullName,
					SubjectId = lessonPlan.SubjectId,
					SubjectName = lessonPlan.Subject.Name,
					Title = lessonPlan.Title,
					Objective = lessonPlan.Objective,
					Note = lessonPlan.Note,
					ActivityCount = lessonPlan.Activities.Count,
					CreatedAt = lessonPlan.CreatedAt,
					UpdatedAt = lessonPlan.UpdatedAt
				};

				return BaseResponse<LessonPlanResponse>.Ok(response);
			}
			catch (Exception ex)
			{
				return BaseResponse<LessonPlanResponse>.Fail($"Error retrieving lesson plan: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<LessonPlanResponse>> CreateLessonPlanAsync(CreateLessonPlanRequest request, Guid userId)
		{
			try
			{
				// Check role:
				var user = await _unitOfWork.Users.GetByIdAsync(userId);
				if (user != null && user.Role == UserRole.Student)
					return BaseResponse<LessonPlanResponse>.Fail($"This action only allow for Teachers", ResponseErrorType.Forbidden);
				// Check if subject exists
				var subject = await _unitOfWork.Subjects.GetByIdAsync(request.SubjectId);
				if (subject == null)
				{
					return BaseResponse<LessonPlanResponse>.Fail("Subject not found", ResponseErrorType.NotFound);
				}

				// Check if lesson plan with same title exists for this user
				var exists = await _unitOfWork.LessonPlans.ExistsByTitleAndUserAsync(request.Title, userId);
				if (exists)
				{
					return BaseResponse<LessonPlanResponse>.Fail($"Lesson plan '{request.Title}' already exists", ResponseErrorType.Conflict);
				}

				var lessonPlan = new LessonPlan
				{
					Id = Guid.NewGuid(),
					UserId = userId,
					SubjectId = request.SubjectId,
					Title = request.Title,
					Objective = request.Objective,
					Note = request.Note
				};

				await _unitOfWork.LessonPlans.AddAsync(lessonPlan);
				await _unitOfWork.SaveChangesAsync();

				// Reload with navigation properties
				lessonPlan = await _unitOfWork.LessonPlans.GetByIdWithActivitiesAsync(lessonPlan.Id);

				var response = new LessonPlanResponse
				{
					Id = lessonPlan!.Id,
					UserId = lessonPlan.UserId,
					UserName = lessonPlan.User.FullName,
					SubjectId = lessonPlan.SubjectId,
					SubjectName = lessonPlan.Subject.Name,
					Title = lessonPlan.Title,
					Objective = lessonPlan.Objective,
					Note = lessonPlan.Note,
					ActivityCount = lessonPlan.Activities.Count,
					CreatedAt = lessonPlan.CreatedAt,
					UpdatedAt = lessonPlan.UpdatedAt
				};

				return BaseResponse<LessonPlanResponse>.Ok(response, "Lesson plan created successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<LessonPlanResponse>.Fail($"Error creating lesson plan: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<LessonPlanResponse>> UpdateLessonPlanAsync(Guid id, UpdateLessonPlanRequest request, Guid userId)
		{
			try
			{
				var lessonPlan = await _unitOfWork.LessonPlans.GetByIdAsync(id);

				if (lessonPlan == null)
				{
					return BaseResponse<LessonPlanResponse>.Fail("Lesson plan not found", ResponseErrorType.NotFound);
				}

				// Check ownership
				if (lessonPlan.UserId != userId)
				{
					return BaseResponse<LessonPlanResponse>.Fail("You don't have permission to update this lesson plan", ResponseErrorType.Forbidden);
				}

				// Check if subject exists
				var subject = await _unitOfWork.Subjects.GetByIdAsync(request.SubjectId);
				if (subject == null)
				{
					return BaseResponse<LessonPlanResponse>.Fail("Subject not found", ResponseErrorType.NotFound);
				}

				// Check if new title conflicts with existing lesson plan
				var exists = await _unitOfWork.LessonPlans.ExistsByTitleAndUserAsync(request.Title, userId, id);
				if (exists)
				{
					return BaseResponse<LessonPlanResponse>.Fail($"Lesson plan '{request.Title}' already exists", ResponseErrorType.Conflict);
				}

				lessonPlan.SubjectId = request.SubjectId;
				lessonPlan.Title = request.Title;
				lessonPlan.Objective = request.Objective;
				lessonPlan.Note = request.Note;

				_unitOfWork.LessonPlans.Update(lessonPlan);
				await _unitOfWork.SaveChangesAsync();

				// Reload with navigation properties
				lessonPlan = await _unitOfWork.LessonPlans.GetByIdWithActivitiesAsync(id);

				var response = new LessonPlanResponse
				{
					Id = lessonPlan!.Id,
					UserId = lessonPlan.UserId,
					UserName = lessonPlan.User.FullName,
					SubjectId = lessonPlan.SubjectId,
					SubjectName = lessonPlan.Subject.Name,
					Title = lessonPlan.Title,
					Objective = lessonPlan.Objective,
					Note = lessonPlan.Note,
					ActivityCount = lessonPlan.Activities.Count,
					CreatedAt = lessonPlan.CreatedAt,
					UpdatedAt = lessonPlan.UpdatedAt
				};

				return BaseResponse<LessonPlanResponse>.Ok(response, "Lesson plan updated successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<LessonPlanResponse>.Fail($"Error updating lesson plan: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse> DeleteLessonPlanAsync(Guid id, Guid userId)
		{
			try
			{
				var lessonPlan = await _unitOfWork.LessonPlans.GetByIdAsync(id);

				if (lessonPlan == null)
				{
					return BaseResponse.Fail("Lesson plan not found", ResponseErrorType.NotFound);
				}

				// Check ownership
				if (lessonPlan.UserId != userId)
				{
					return BaseResponse.Fail("You don't have permission to delete this lesson plan", ResponseErrorType.Forbidden);
				}

				_unitOfWork.LessonPlans.Remove(lessonPlan);
				await _unitOfWork.SaveChangesAsync();

				return BaseResponse.Ok("Lesson plan deleted successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse.Fail($"Error deleting lesson plan: {ex.Message}", ResponseErrorType.InternalError);
			}
		}
	}
}
