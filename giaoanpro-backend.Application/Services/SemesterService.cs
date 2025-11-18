using giaoanpro_backend.Application.DTOs.Requests.Semesters;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Semesters;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Services
{
	public class SemesterService : ISemesterService
	{
		private readonly IUnitOfWork _unitOfWork;

		public SemesterService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<PagedResult<SemesterResponse>>> GetSemestersAsync(GetSemestersQuery query)
		{
			try
			{
				var currentDate = DateTime.UtcNow;

				var (semesters, totalCount) = await _unitOfWork.Semesters.GetPagedAsync(
					filter: s =>
						(!query.IsActive.HasValue || (query.IsActive.Value ? (s.StartDate <= currentDate && s.EndDate >= currentDate) : true)) &&
						(string.IsNullOrEmpty(query.Name) || s.Name.ToLower().Contains(query.Name.ToLower())) &&
						(!query.StartDate.HasValue || s.StartDate >= query.StartDate.Value) &&
						(!query.EndDate.HasValue || s.EndDate <= query.EndDate.Value),
					orderBy: q => q.OrderByDescending(s => s.StartDate),
					pageNumber: query.PageNumber,
					pageSize: query.PageSize,
					asNoTracking: true
				);

				var semesterResponses = semesters.Select(s => new SemesterResponse
				{
					Id = s.Id,
					Name = s.Name,
					StartDate = s.StartDate,
					EndDate = s.EndDate,
					CreatedAt = s.CreatedAt,
					UpdatedAt = s.UpdatedAt
				}).ToList();

				var pagedResult = new PagedResult<SemesterResponse>(semesterResponses, query.PageNumber, query.PageSize, totalCount);

				return BaseResponse<PagedResult<SemesterResponse>>.Ok(pagedResult);
			}
			catch (Exception ex)
			{
				return BaseResponse<PagedResult<SemesterResponse>>.Fail($"Error retrieving semesters: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<SemesterResponse>> GetSemesterByIdAsync(Guid id)
		{
			try
			{
				var semester = await _unitOfWork.Semesters.GetByIdAsync(id);

				if (semester == null)
				{
					return BaseResponse<SemesterResponse>.Fail("Semester not found", ResponseErrorType.NotFound);
				}

				var response = new SemesterResponse
				{
					Id = semester.Id,
					Name = semester.Name,
					StartDate = semester.StartDate,
					EndDate = semester.EndDate,
					CreatedAt = semester.CreatedAt,
					UpdatedAt = semester.UpdatedAt
				};

				return BaseResponse<SemesterResponse>.Ok(response);
			}
			catch (Exception ex)
			{
				return BaseResponse<SemesterResponse>.Fail($"Error retrieving semester: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<SemesterResponse>> CreateSemesterAsync(CreateSemesterRequest request)
		{
			try
			{
				// Check if semester name already exists
				var nameExists = await _unitOfWork.Semesters.ExistsByNameAsync(request.Name);
				if (nameExists)
				{
					return BaseResponse<SemesterResponse>.Fail($"Semester with name '{request.Name}' already exists", ResponseErrorType.Conflict);
				}

				// Check for date overlap
				var hasOverlap = await _unitOfWork.Semesters.HasDateOverlapAsync(request.StartDate, request.EndDate);
				if (hasOverlap)
				{
					return BaseResponse<SemesterResponse>.Fail("Semester dates overlap with an existing semester", ResponseErrorType.Conflict);
				}

				var semester = new Semester
				{
					Id = Guid.NewGuid(),
					Name = request.Name,
					StartDate = request.StartDate,
					EndDate = request.EndDate
				};

				await _unitOfWork.Semesters.AddAsync(semester);
				await _unitOfWork.SaveChangesAsync();

				var response = new SemesterResponse
				{
					Id = semester.Id,
					Name = semester.Name,
					StartDate = semester.StartDate,
					EndDate = semester.EndDate,
					CreatedAt = semester.CreatedAt,
					UpdatedAt = semester.UpdatedAt
				};

				return BaseResponse<SemesterResponse>.Ok(response, "Semester created successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<SemesterResponse>.Fail($"Error creating semester: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<SemesterResponse>> UpdateSemesterAsync(Guid id, UpdateSemesterRequest request)
		{
			try
			{
				var semester = await _unitOfWork.Semesters.GetByIdAsync(id);

				if (semester == null)
				{
					return BaseResponse<SemesterResponse>.Fail("Semester not found", ResponseErrorType.NotFound);
				}

				// Check if new name conflicts with existing semester
				var nameExists = await _unitOfWork.Semesters.ExistsByNameAsync(request.Name, id);
				if (nameExists)
				{
					return BaseResponse<SemesterResponse>.Fail($"Semester with name '{request.Name}' already exists", ResponseErrorType.Conflict);
				}

				// Check for date overlap
				var hasOverlap = await _unitOfWork.Semesters.HasDateOverlapAsync(request.StartDate, request.EndDate, id);
				if (hasOverlap)
				{
					return BaseResponse<SemesterResponse>.Fail("Semester dates overlap with an existing semester", ResponseErrorType.Conflict);
				}

				semester.Name = request.Name;
				semester.StartDate = request.StartDate;
				semester.EndDate = request.EndDate;

				_unitOfWork.Semesters.Update(semester);
				await _unitOfWork.SaveChangesAsync();

				var response = new SemesterResponse
				{
					Id = semester.Id,
					Name = semester.Name,
					StartDate = semester.StartDate,
					EndDate = semester.EndDate,
					CreatedAt = semester.CreatedAt,
					UpdatedAt = semester.UpdatedAt
				};

				return BaseResponse<SemesterResponse>.Ok(response, "Semester updated successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<SemesterResponse>.Fail($"Error updating semester: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse> DeleteSemesterAsync(Guid id)
		{
			try
			{
				var semester = await _unitOfWork.Semesters.GetByIdAsync(id);

				if (semester == null)
				{
					return BaseResponse.Fail("Semester not found", ResponseErrorType.NotFound);
				}

				// Check if semester has classes
				var hasClasses = await _unitOfWork.Classes.AnyAsync(c => c.SemesterId == id);

				if (hasClasses)
				{
					return BaseResponse.Fail("Cannot delete semester because it has associated classes", ResponseErrorType.Conflict);
				}

				_unitOfWork.Semesters.Remove(semester);
				await _unitOfWork.SaveChangesAsync();

				return BaseResponse.Ok("Semester deleted successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse.Fail($"Error deleting semester: {ex.Message}", ResponseErrorType.InternalError);
			}
		}
	}
}
