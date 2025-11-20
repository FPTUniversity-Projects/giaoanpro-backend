using giaoanpro_backend.Application.DTOs.Requests.Classes;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Classes;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Application.Services
{
	public class ClassService : IClassService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ClassService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<PagedResult<ClassResponse>>> GetClassesAsync(GetClassesQuery query)
		{
			try
			{
				var (classes, totalCount) = await _unitOfWork.Classes.GetPagedAsync(
					filter: c =>
						(string.IsNullOrEmpty(query.Name) || c.Name.ToLower().Contains(query.Name.ToLower())) &&
						(!query.TeacherId.HasValue || c.TeacherId == query.TeacherId.Value) &&
						(!query.GradeId.HasValue || c.GradeId == query.GradeId.Value) &&
						(!query.SemesterId.HasValue || c.SemesterId == query.SemesterId.Value),
					include: q => q
						.Include(c => c.Teacher)
						.Include(c => c.Grade)
						.Include(c => c.Semester)
						.Include(c => c.Members),
					orderBy: q => q.OrderBy(c => c.Name),
					pageNumber: query.PageNumber,
					pageSize: query.PageSize,
					asNoTracking: true
				);

				var classResponses = classes.Select(c => new ClassResponse
				{
					Id = c.Id,
					TeacherId = c.TeacherId,
					TeacherName = c.Teacher.FullName,
					GradeId = c.GradeId,
					GradeLevel = c.Grade.Level,
					SemesterId = c.SemesterId,
					SemesterName = c.Semester.Name,
					Name = c.Name,
					MemberCount = c.Members.Count,
					CreatedAt = c.CreatedAt,
					UpdatedAt = c.UpdatedAt
				}).ToList();

				var pagedResult = new PagedResult<ClassResponse>(classResponses, query.PageNumber, query.PageSize, totalCount);

				return BaseResponse<PagedResult<ClassResponse>>.Ok(pagedResult);
			}
			catch (Exception ex)
			{
				return BaseResponse<PagedResult<ClassResponse>>.Fail($"Error retrieving classes: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<ClassResponse>> GetClassByIdAsync(Guid id)
		{
			try
			{
				var classEntity = await _unitOfWork.Classes.GetByConditionAsync(
					c => c.Id == id,
					include: q => q
						.Include(c => c.Teacher)
						.Include(c => c.Grade)
						.Include(c => c.Semester)
						.Include(c => c.Members)
				);

				if (classEntity == null)
				{
					return BaseResponse<ClassResponse>.Fail("Class not found", ResponseErrorType.NotFound);
				}

				var response = new ClassResponse
				{
					Id = classEntity.Id,
					TeacherId = classEntity.TeacherId,
					TeacherName = classEntity.Teacher.FullName,
					GradeId = classEntity.GradeId,
					GradeLevel = classEntity.Grade.Level,
					SemesterId = classEntity.SemesterId,
					SemesterName = classEntity.Semester.Name,
					Name = classEntity.Name,
					MemberCount = classEntity.Members.Count,
					CreatedAt = classEntity.CreatedAt,
					UpdatedAt = classEntity.UpdatedAt
				};

				return BaseResponse<ClassResponse>.Ok(response);
			}
			catch (Exception ex)
			{
				return BaseResponse<ClassResponse>.Fail($"Error retrieving class: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<ClassResponse>> CreateClassAsync(CreateClassRequest request)
		{
			try
			{
				// Check if teacher exists
				var teacher = await _unitOfWork.Users.GetByIdAsync(request.TeacherId);
				if (teacher == null)
				{
					return BaseResponse<ClassResponse>.Fail("Teacher not found", ResponseErrorType.NotFound);
				}

				// Check if grade exists
				var grade = await _unitOfWork.Grades.GetByIdAsync(request.GradeId);
				if (grade == null)
				{
					return BaseResponse<ClassResponse>.Fail("Grade not found", ResponseErrorType.NotFound);
				}

				// Check if semester exists
				var semester = await _unitOfWork.Semesters.GetByIdAsync(request.SemesterId);
				if (semester == null)
				{
					return BaseResponse<ClassResponse>.Fail("Semester not found", ResponseErrorType.NotFound);
				}

				// Check if class name already exists in this semester
				var exists = await _unitOfWork.Classes.ExistsByNameAndSemesterAsync(request.Name, request.SemesterId);
				if (exists)
				{
					return BaseResponse<ClassResponse>.Fail($"Class '{request.Name}' already exists in this semester", ResponseErrorType.Conflict);
				}

				var classEntity = new Class
				{
					Id = Guid.NewGuid(),
					TeacherId = request.TeacherId,
					GradeId = request.GradeId,
					SemesterId = request.SemesterId,
					Name = request.Name
				};

				await _unitOfWork.Classes.AddAsync(classEntity);
				await _unitOfWork.SaveChangesAsync();

				// Reload with navigation properties
				classEntity = await _unitOfWork.Classes.GetByConditionAsync(
					c => c.Id == classEntity.Id,
					include: q => q
						.Include(c => c.Teacher)
						.Include(c => c.Grade)
						.Include(c => c.Semester)
						.Include(c => c.Members)
				);

				var response = new ClassResponse
				{
					Id = classEntity!.Id,
					TeacherId = classEntity.TeacherId,
					TeacherName = classEntity.Teacher.FullName,
					GradeId = classEntity.GradeId,
					GradeLevel = classEntity.Grade.Level,
					SemesterId = classEntity.SemesterId,
					SemesterName = classEntity.Semester.Name,
					Name = classEntity.Name,
					MemberCount = classEntity.Members.Count,
					CreatedAt = classEntity.CreatedAt,
					UpdatedAt = classEntity.UpdatedAt
				};

				return BaseResponse<ClassResponse>.Ok(response, "Class created successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<ClassResponse>.Fail($"Error creating class: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<ClassResponse>> UpdateClassAsync(Guid id, UpdateClassRequest request)
		{
			try
			{
				var classEntity = await _unitOfWork.Classes.GetByIdAsync(id);

				if (classEntity == null)
				{
					return BaseResponse<ClassResponse>.Fail("Class not found", ResponseErrorType.NotFound);
				}

				// Check if teacher exists
				var teacher = await _unitOfWork.Users.GetByIdAsync(request.TeacherId);
				if (teacher == null)
				{
					return BaseResponse<ClassResponse>.Fail("Teacher not found", ResponseErrorType.NotFound);
				}

				// Check if grade exists
				var grade = await _unitOfWork.Grades.GetByIdAsync(request.GradeId);
				if (grade == null)
				{
					return BaseResponse<ClassResponse>.Fail("Grade not found", ResponseErrorType.NotFound);
				}

				// Check if semester exists
				var semester = await _unitOfWork.Semesters.GetByIdAsync(request.SemesterId);
				if (semester == null)
				{
					return BaseResponse<ClassResponse>.Fail("Semester not found", ResponseErrorType.NotFound);
				}

				// Check if new name conflicts with existing class
				var exists = await _unitOfWork.Classes.ExistsByNameAndSemesterAsync(request.Name, request.SemesterId, id);
				if (exists)
				{
					return BaseResponse<ClassResponse>.Fail($"Class '{request.Name}' already exists in this semester", ResponseErrorType.Conflict);
				}

				classEntity.TeacherId = request.TeacherId;
				classEntity.GradeId = request.GradeId;
				classEntity.SemesterId = request.SemesterId;
				classEntity.Name = request.Name;

				_unitOfWork.Classes.Update(classEntity);
				await _unitOfWork.SaveChangesAsync();

				// Reload with navigation properties
				classEntity = await _unitOfWork.Classes.GetByConditionAsync(
					c => c.Id == id,
					include: q => q
						.Include(c => c.Teacher)
						.Include(c => c.Grade)
						.Include(c => c.Semester)
						.Include(c => c.Members)
				);

				var response = new ClassResponse
				{
					Id = classEntity!.Id,
					TeacherId = classEntity.TeacherId,
					TeacherName = classEntity.Teacher.FullName,
					GradeId = classEntity.GradeId,
					GradeLevel = classEntity.Grade.Level,
					SemesterId = classEntity.SemesterId,
					SemesterName = classEntity.Semester.Name,
					Name = classEntity.Name,
					MemberCount = classEntity.Members.Count,
					CreatedAt = classEntity.CreatedAt,
					UpdatedAt = classEntity.UpdatedAt
				};

				return BaseResponse<ClassResponse>.Ok(response, "Class updated successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<ClassResponse>.Fail($"Error updating class: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse> DeleteClassAsync(Guid id)
		{
			try
			{
				var classEntity = await _unitOfWork.Classes.GetByIdAsync(id);

				if (classEntity == null)
				{
					return BaseResponse.Fail("Class not found", ResponseErrorType.NotFound);
				}

				_unitOfWork.Classes.Remove(classEntity);
				await _unitOfWork.SaveChangesAsync();

				return BaseResponse.Ok("Class deleted successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse.Fail($"Error deleting class: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<PagedResult<ClassMemberResponse>>> GetClassMembersByClassIdAsync(Guid id)
		{
			try
			{
				if (id == Guid.Empty)
					return BaseResponse<PagedResult<ClassMemberResponse>>.Fail("Invalid class id", ResponseErrorType.BadRequest);

				var classEntity = await _unitOfWork.Classes.GetWithMembersAsync(id);
				if (classEntity == null)
				{
					return BaseResponse<PagedResult<ClassMemberResponse>>.Fail("Class not found", ResponseErrorType.NotFound);
				}

				var members = classEntity.Members ?? new List<ClassMember>();

				var memberResponses = members
					.OrderBy(m => m.Student?.FullName)
					.Select(m => new ClassMemberResponse
					{
						Id = m.Student.Id,
						FullName = m.Student.FullName,
						Email = m.Student.Email,
						Role = m.Student.Role.ToString()
					})
					.ToList();

				int totalCount = memberResponses.Count;
				int pageNumber = 1;
				int pageSize = Math.Max(1, totalCount);

				var pagedResult = new PagedResult<ClassMemberResponse>(memberResponses, pageNumber, pageSize, totalCount);

				return BaseResponse<PagedResult<ClassMemberResponse>>.Ok(pagedResult);
			}
			catch (Exception ex)
			{
				return BaseResponse<PagedResult<ClassMemberResponse>>.Fail($"Error retrieving class members: {ex.Message}", ResponseErrorType.InternalError);
			}
		}
	}
}
