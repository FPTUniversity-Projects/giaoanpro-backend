using giaoanpro_backend.Application.DTOs.Requests.Subjects;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Subjects;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Application.Services
{
	public class SubjectService : ISubjectService
	{
		private readonly IUnitOfWork _unitOfWork;

		public SubjectService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<PagedResult<SubjectResponse>>> GetSubjectsAsync(GetSubjectsQuery query)
		{
			try
			{
				var (subjects, totalCount) = await _unitOfWork.Subjects.GetPagedAsync(
					filter: s =>
						(string.IsNullOrEmpty(query.Name) || s.Name.ToLower().Contains(query.Name.ToLower())) &&
						(!query.GradeId.HasValue || s.GradeId == query.GradeId.Value),
					include: q => q.Include(s => s.Grade),
					orderBy: q => q.OrderBy(s => s.Name),
					pageNumber: query.PageNumber,
					pageSize: query.PageSize,
					asNoTracking: true
				);

				var subjectResponses = subjects.Select(s => new SubjectResponse
				{
					Id = s.Id,
					GradeId = s.GradeId,
					GradeLevel = $"Grade {s.Grade.Level}",
					Name = s.Name,
					Description = s.Description,
					CreatedAt = s.CreatedAt,
					UpdatedAt = s.UpdatedAt
				}).ToList();

				var pagedResult = new PagedResult<SubjectResponse>(subjectResponses, query.PageNumber, query.PageSize, totalCount);

				return BaseResponse<PagedResult<SubjectResponse>>.Ok(pagedResult);
			}
			catch (Exception ex)
			{
				return BaseResponse<PagedResult<SubjectResponse>>.Fail($"Error retrieving subjects: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<SubjectResponse>> GetSubjectByIdAsync(Guid id)
		{
			try
			{
				var subject = await _unitOfWork.Subjects.GetByConditionAsync(
					s => s.Id == id,
					include: q => q.Include(s => s.Grade)
				);

				if (subject == null)
				{
					return BaseResponse<SubjectResponse>.Fail("Subject not found", ResponseErrorType.NotFound);
				}

				var response = new SubjectResponse
				{
					Id = subject.Id,
					GradeId = subject.GradeId,
					GradeLevel = $"Grade {subject.Grade.Level}",
					Name = subject.Name,
					Description = subject.Description,
					CreatedAt = subject.CreatedAt,
					UpdatedAt = subject.UpdatedAt
				};

				return BaseResponse<SubjectResponse>.Ok(response);
			}
			catch (Exception ex)
			{
				return BaseResponse<SubjectResponse>.Fail($"Error retrieving subject: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<SubjectResponse>> CreateSubjectAsync(CreateSubjectRequest request)
		{
			try
			{
				// Check if grade exists
				var grade = await _unitOfWork.Grades.GetByIdAsync(request.GradeId);
				if (grade == null)
				{
					return BaseResponse<SubjectResponse>.Fail("Grade not found", ResponseErrorType.NotFound);
				}

				// Check if subject with same name exists for this grade
				var exists = await _unitOfWork.Subjects.ExistsByNameAndGradeAsync(request.Name, request.GradeId);
				if (exists)
				{
					return BaseResponse<SubjectResponse>.Fail($"Subject '{request.Name}' already exists for this grade", ResponseErrorType.Conflict);
				}

				var subject = new Subject
				{
					Id = Guid.NewGuid(),
					GradeId = request.GradeId,
					SyllabusId = Guid.Empty, // Will be set when syllabus is created
					Name = request.Name,
					Description = request.Description
				};

				await _unitOfWork.Subjects.AddAsync(subject);
				await _unitOfWork.SaveChangesAsync();

				// Reload with navigation properties
				subject = await _unitOfWork.Subjects.GetByConditionAsync(
					s => s.Id == subject.Id,
					include: q => q.Include(s => s.Grade)
				);

				var response = new SubjectResponse
				{
					Id = subject!.Id,
					GradeId = subject.GradeId,
					GradeLevel = $"Grade {subject.Grade.Level}",
					Name = subject.Name,
					Description = subject.Description,
					CreatedAt = subject.CreatedAt,
					UpdatedAt = subject.UpdatedAt
				};

				return BaseResponse<SubjectResponse>.Ok(response, "Subject created successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<SubjectResponse>.Fail($"Error creating subject: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<SubjectResponse>> UpdateSubjectAsync(Guid id, UpdateSubjectRequest request)
		{
			try
			{
				var subject = await _unitOfWork.Subjects.GetByIdAsync(id);

				if (subject == null)
				{
					return BaseResponse<SubjectResponse>.Fail("Subject not found", ResponseErrorType.NotFound);
				}

				// Check if grade exists
				var grade = await _unitOfWork.Grades.GetByIdAsync(request.GradeId);
				if (grade == null)
				{
					return BaseResponse<SubjectResponse>.Fail("Grade not found", ResponseErrorType.NotFound);
				}

				// Check if new name conflicts with existing subject
				var exists = await _unitOfWork.Subjects.ExistsByNameAndGradeAsync(request.Name, request.GradeId, id);
				if (exists)
				{
					return BaseResponse<SubjectResponse>.Fail($"Subject '{request.Name}' already exists for this grade", ResponseErrorType.Conflict);
				}

				subject.GradeId = request.GradeId;
				subject.Name = request.Name;
				subject.Description = request.Description;

				_unitOfWork.Subjects.Update(subject);
				await _unitOfWork.SaveChangesAsync();

				// Reload with navigation properties
				subject = await _unitOfWork.Subjects.GetByConditionAsync(
					s => s.Id == id,
					include: q => q.Include(s => s.Grade)
				);

				var response = new SubjectResponse
				{
					Id = subject!.Id,
					GradeId = subject.GradeId,
					GradeLevel = $"Grade {subject.Grade.Level}",
					Name = subject.Name,
					Description = subject.Description,
					CreatedAt = subject.CreatedAt,
					UpdatedAt = subject.UpdatedAt
				};

				return BaseResponse<SubjectResponse>.Ok(response, "Subject updated successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<SubjectResponse>.Fail($"Error updating subject: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse> DeleteSubjectAsync(Guid id)
		{
			try
			{
				var subject = await _unitOfWork.Subjects.GetByIdAsync(id);

				if (subject == null)
				{
					return BaseResponse.Fail("Subject not found", ResponseErrorType.NotFound);
				}

				// Check if subject has syllabus, lesson plans, or exam matrices
				var hasSyllabus = await _unitOfWork.Syllabuses.AnyAsync(s => s.SubjectId == id);

				if (hasSyllabus)
				{
					return BaseResponse.Fail("Cannot delete subject because it has associated syllabus, lesson plans, or exam matrices", ResponseErrorType.Conflict);
				}

				_unitOfWork.Subjects.Remove(subject);
				await _unitOfWork.SaveChangesAsync();

				return BaseResponse.Ok("Subject deleted successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse.Fail($"Error deleting subject: {ex.Message}", ResponseErrorType.InternalError);
			}
		}
	}
}
