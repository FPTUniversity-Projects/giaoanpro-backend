using giaoanpro_backend.Application.DTOs.Requests.Classes;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Classes;
using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Services
{
	public class ClassEnrollmentService : IClassEnrollmentService
	{
		private readonly IClassMemberRepository _classMemberRepository;
		private readonly IClassRepository _classRepository;
		private readonly IUserRepository _userRepository;

		public ClassEnrollmentService(IClassMemberRepository classMemberRepository, IClassRepository classRepository, IUserRepository userRepository)
		{
			_classMemberRepository = classMemberRepository;
			_classRepository = classRepository;
			_userRepository = userRepository;
		}

		public async Task<BaseResponse<string>> EnrollStudentToClassAsync(Guid classId, Guid studentId)
		{
			try
			{
				if (classId == Guid.Empty || studentId == Guid.Empty)
					return BaseResponse<string>.Fail("Invalid class or student id", ResponseErrorType.BadRequest);

				var classExists = await _classRepository.AnyAsync(c => c.Id == classId);
				if (!classExists)
					return BaseResponse<string>.Fail("Class not found", ResponseErrorType.NotFound);

				var studentExists = await _userRepository.AnyAsync(u => u.Id == studentId);
				if (!studentExists)
					return BaseResponse<string>.Fail("Student not found", ResponseErrorType.NotFound);

				// Check already enrolled
				var exists = await _classMemberRepository.AnyAsync(cm => cm.ClassId == classId && cm.StudentId == studentId);
				if (exists)
					return BaseResponse<string>.Fail("Student already enrolled in class", ResponseErrorType.Conflict);

				var member = new ClassMember
				{
					ClassId = classId,
					StudentId = studentId
				};

				await _classMemberRepository.AddAsync(member);
				var saved = await _classMemberRepository.SaveChangesAsync();
				if (!saved)
					return BaseResponse<string>.Fail("Failed to enroll student", ResponseErrorType.InternalError);

				return BaseResponse<string>.Ok("Student enrolled successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<string>.Fail($"Error enrolling student: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<PagedResult<ClassResponse>>> GetEnrolledClassesByStudentIdAsync(Guid studentId, GetEnrolledClassQuery query)
		{
			try
			{
				if (studentId == Guid.Empty)
					return BaseResponse<PagedResult<ClassResponse>>.Fail("Invalid student id", ResponseErrorType.BadRequest);

				query ??= new GetEnrolledClassQuery();

				// Delegate EF-heavy query to repository
				var (members, totalCount) = await _classMemberRepository.GetPagedByStudentAsync(
					studentId,
					query.Name,
					query.TeacherId,
					query.GradeId,
					query.SemesterId,
					query.PageNumber,
					query.PageSize
				);

				var classResponses = members.Select(m => new ClassResponse
				{
					Id = m.Class.Id,
					TeacherId = m.Class.TeacherId,
					TeacherName = m.Class.Teacher?.FullName ?? string.Empty,
					GradeId = m.Class.GradeId,
					GradeLevel = m.Class.Grade?.Level ?? 0,
					SemesterId = m.Class.SemesterId,
					SemesterName = m.Class.Semester?.Name ?? string.Empty,
					Name = m.Class.Name,
					MemberCount = m.Class.Members?.Count ?? 0,
					CreatedAt = m.Class.CreatedAt,
					UpdatedAt = m.Class.UpdatedAt
				}).ToList();

				var pagedResult = new PagedResult<ClassResponse>(classResponses, query.PageNumber, query.PageSize, totalCount);

				return BaseResponse<PagedResult<ClassResponse>>.Ok(pagedResult);
			}
			catch (Exception ex)
			{
				return BaseResponse<PagedResult<ClassResponse>>.Fail($"Error retrieving enrolled classes: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<string>> RemoveStudentFromClassAsync(Guid classId, Guid studentId)
		{
			try
			{
				if (classId == Guid.Empty || studentId == Guid.Empty)
					return BaseResponse<string>.Fail("Invalid class or student id", ResponseErrorType.BadRequest);

				var classExists = await _classRepository.AnyAsync(c => c.Id == classId);
				if (!classExists)
					return BaseResponse<string>.Fail("Class not found", ResponseErrorType.NotFound);

				var studentExists = await _userRepository.AnyAsync(u => u.Id == studentId);
				if (!studentExists)
					return BaseResponse<string>.Fail("Student not found", ResponseErrorType.NotFound);

				var member = await _classMemberRepository.GetByConditionAsync(cm => cm.ClassId == classId && cm.StudentId == studentId);
				if (member == null)
					return BaseResponse<string>.Fail("Enrollment not found", ResponseErrorType.NotFound);

				_classMemberRepository.Remove(member);
				var saved = await _classMemberRepository.SaveChangesAsync();
				if (!saved)
					return BaseResponse<string>.Fail("Failed to remove student from class", ResponseErrorType.InternalError);

				return BaseResponse<string>.Ok("Student removed from class successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<string>.Fail($"Error removing student from class: {ex.Message}", ResponseErrorType.InternalError);
			}
		}
	}
}
