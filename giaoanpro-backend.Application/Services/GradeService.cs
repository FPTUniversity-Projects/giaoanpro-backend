using giaoanpro_backend.Application.DTOs.Requests.Grades;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Grades;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Application.Services
{
	public class GradeService : IGradeService
	{
		private readonly IUnitOfWork _unitOfWork;

		public GradeService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<PagedResult<GradeResponse>>> GetGradesAsync(GetGradesQuery query)
		{
			try
			{
				var (grades, totalCount) = await _unitOfWork.Grades.GetPagedAsync(
					filter: query.Level.HasValue ? g => g.Level == query.Level.Value : null,
					orderBy: q => q.OrderBy(g => g.Level),
					pageNumber: query.PageNumber,
					pageSize: query.PageSize,
					asNoTracking: true
				);

				var gradeResponses = grades.Select(g => new GradeResponse
				{
					Id = g.Id,
					Level = g.Level,
					CreatedAt = g.CreatedAt,
					UpdatedAt = g.UpdatedAt
				}).ToList();

				var pagedResult = new PagedResult<GradeResponse>(gradeResponses, query.PageNumber, query.PageSize, totalCount);

				return BaseResponse<PagedResult<GradeResponse>>.Ok(pagedResult);
			}
			catch (Exception ex)
			{
				return BaseResponse<PagedResult<GradeResponse>>.Fail($"Error retrieving grades: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<GradeResponse>> GetGradeByIdAsync(Guid id)
		{
			try
			{
				var grade = await _unitOfWork.Grades.GetByIdAsync(id);

				if (grade == null)
				{
					return BaseResponse<GradeResponse>.Fail("Grade not found", ResponseErrorType.NotFound);
				}

				var response = new GradeResponse
				{
					Id = grade.Id,
					Level = grade.Level,
					CreatedAt = grade.CreatedAt,
					UpdatedAt = grade.UpdatedAt
				};

				return BaseResponse<GradeResponse>.Ok(response);
			}
			catch (Exception ex)
			{
				return BaseResponse<GradeResponse>.Fail($"Error retrieving grade: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<GradeResponse>> CreateGradeAsync(CreateGradeRequest request)
		{
			try
			{
				// Check if grade level already exists
				var exists = await _unitOfWork.Grades.ExistsByLevelAsync(request.Level);
				if (exists)
				{
					return BaseResponse<GradeResponse>.Fail($"Grade level {request.Level} already exists", ResponseErrorType.Conflict);
				}

				var grade = new Grade
				{
					Id = Guid.NewGuid(),
					Level = request.Level
				};

				await _unitOfWork.Grades.AddAsync(grade);
				await _unitOfWork.SaveChangesAsync();

				var response = new GradeResponse
				{
					Id = grade.Id,
					Level = grade.Level,
					CreatedAt = grade.CreatedAt,
					UpdatedAt = grade.UpdatedAt
				};

				return BaseResponse<GradeResponse>.Ok(response, "Grade created successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<GradeResponse>.Fail($"Error creating grade: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<GradeResponse>> UpdateGradeAsync(Guid id, UpdateGradeRequest request)
		{
			try
			{
				var grade = await _unitOfWork.Grades.GetByIdAsync(id);

				if (grade == null)
				{
					return BaseResponse<GradeResponse>.Fail("Grade not found", ResponseErrorType.NotFound);
				}

				// Check if new level conflicts with existing grade
				var exists = await _unitOfWork.Grades.ExistsByLevelAsync(request.Level, id);
				if (exists)
				{
					return BaseResponse<GradeResponse>.Fail($"Grade level {request.Level} already exists", ResponseErrorType.Conflict);
				}

				grade.Level = request.Level;

				_unitOfWork.Grades.Update(grade);
				await _unitOfWork.SaveChangesAsync();

				var response = new GradeResponse
				{
					Id = grade.Id,
					Level = grade.Level,
					CreatedAt = grade.CreatedAt,
					UpdatedAt = grade.UpdatedAt
				};

				return BaseResponse<GradeResponse>.Ok(response, "Grade updated successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<GradeResponse>.Fail($"Error updating grade: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse> DeleteGradeAsync(Guid id)
		{
			try
			{
				var grade = await _unitOfWork.Grades.GetByIdAsync(id);

				if (grade == null)
				{
					return BaseResponse.Fail("Grade not found", ResponseErrorType.NotFound);
				}

				// Check if grade has subjects or classes
				var hasSubjects = await _unitOfWork.Subjects.AnyAsync(s => s.GradeId == id);
				var hasClasses = await _unitOfWork.Classes.AnyAsync(c => c.GradeId == id);

				if (hasSubjects || hasClasses)
				{
					return BaseResponse.Fail("Cannot delete grade because it has associated subjects or classes", ResponseErrorType.Conflict);
				}

				_unitOfWork.Grades.Remove(grade);
				await _unitOfWork.SaveChangesAsync();

				return BaseResponse.Ok("Grade deleted successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse.Fail($"Error deleting grade: {ex.Message}", ResponseErrorType.InternalError);
			}
		}
	}
}
