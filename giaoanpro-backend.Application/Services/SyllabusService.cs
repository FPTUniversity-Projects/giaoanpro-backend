using giaoanpro_backend.Application.DTOs.Requests.Syllabuses;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Syllabuses;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Application.Services
{
	public class SyllabusService : ISyllabusService
	{
		private readonly IUnitOfWork _unitOfWork;

		public SyllabusService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<BaseResponse<PagedResult<SyllabusResponse>>> GetSyllabusesAsync(GetSyllabusesQuery query)
		{
			try
			{
				var (syllabuses, totalCount) = await _unitOfWork.Syllabuses.GetPagedAsync(
					filter: s =>
						(string.IsNullOrEmpty(query.Name) || s.Name.ToLower().Contains(query.Name.ToLower())) &&
						(!query.SubjectId.HasValue || s.SubjectId == query.SubjectId.Value),
					include: q => q.Include(s => s.Subject),
					orderBy: q => q.OrderBy(s => s.Name),
					pageNumber: query.PageNumber,
					pageSize: query.PageSize,
					asNoTracking: true
				);

				var syllabusResponses = syllabuses.Select(s => new SyllabusResponse
				{
					Id = s.Id,
					SubjectId = s.SubjectId,
					SubjectName = s.Subject.Name,
					Name = s.Name,
					Description = s.Description,
					PdfUrl = s.PdfUrl,
					CreatedAt = s.CreatedAt,
					UpdatedAt = s.UpdatedAt
				}).ToList();

				var pagedResult = new PagedResult<SyllabusResponse>(syllabusResponses, query.PageNumber, query.PageSize, totalCount);

				return BaseResponse<PagedResult<SyllabusResponse>>.Ok(pagedResult);
			}
			catch (Exception ex)
			{
				return BaseResponse<PagedResult<SyllabusResponse>>.Fail($"Error retrieving syllabuses: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<SyllabusResponse>> GetSyllabusByIdAsync(Guid id)
		{
			try
			{
				var syllabus = await _unitOfWork.Syllabuses.GetByConditionAsync(
					s => s.Id == id,
					include: q => q.Include(s => s.Subject)
				);

				if (syllabus == null)
				{
					return BaseResponse<SyllabusResponse>.Fail("Syllabus not found", ResponseErrorType.NotFound);
				}

				var response = new SyllabusResponse
				{
					Id = syllabus.Id,
					SubjectId = syllabus.SubjectId,
					SubjectName = syllabus.Subject.Name,
					Name = syllabus.Name,
					Description = syllabus.Description,
					PdfUrl = syllabus.PdfUrl,
					CreatedAt = syllabus.CreatedAt,
					UpdatedAt = syllabus.UpdatedAt
				};

				return BaseResponse<SyllabusResponse>.Ok(response);
			}
			catch (Exception ex)
			{
				return BaseResponse<SyllabusResponse>.Fail($"Error retrieving syllabus: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<SyllabusResponse>> GetSyllabusBySubjectIdAsync(Guid subjectId)
		{
			try
			{
				var syllabus = await _unitOfWork.Syllabuses.GetBySubjectIdAsync(subjectId);

				if (syllabus == null)
				{
					return BaseResponse<SyllabusResponse>.Fail("Syllabus not found for this subject", ResponseErrorType.NotFound);
				}

				var response = new SyllabusResponse
				{
					Id = syllabus.Id,
					SubjectId = syllabus.SubjectId,
					SubjectName = syllabus.Subject.Name,
					Name = syllabus.Name,
					Description = syllabus.Description,
					PdfUrl = syllabus.PdfUrl,
					CreatedAt = syllabus.CreatedAt,
					UpdatedAt = syllabus.UpdatedAt
				};

				return BaseResponse<SyllabusResponse>.Ok(response);
			}
			catch (Exception ex)
			{
				return BaseResponse<SyllabusResponse>.Fail($"Error retrieving syllabus: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<SyllabusResponse>> CreateSyllabusAsync(CreateSyllabusRequest request)
		{
			try
			{
				// Check if subject exists
				var subject = await _unitOfWork.Subjects.GetByIdAsync(request.SubjectId);
				if (subject == null)
				{
					return BaseResponse<SyllabusResponse>.Fail("Subject not found", ResponseErrorType.NotFound);
				}

				// Check if syllabus already exists for this subject
				var exists = await _unitOfWork.Syllabuses.ExistsBySubjectIdAsync(request.SubjectId);
				if (exists)
				{
					return BaseResponse<SyllabusResponse>.Fail("A syllabus already exists for this subject", ResponseErrorType.Conflict);
				}

				var syllabus = new Syllabus
				{
					Id = Guid.NewGuid(),
					SubjectId = request.SubjectId,
					Name = request.Name,
					Description = request.Description,
					PdfUrl = request.PdfUrl
				};

				await _unitOfWork.Syllabuses.AddAsync(syllabus);

				// Update subject with syllabus ID
				subject.SyllabusId = syllabus.Id;
				_unitOfWork.Subjects.Update(subject);

				await _unitOfWork.SaveChangesAsync();

				// Reload with navigation properties
				syllabus = await _unitOfWork.Syllabuses.GetByConditionAsync(
					s => s.Id == syllabus.Id,
					include: q => q.Include(s => s.Subject)
				);

				var response = new SyllabusResponse
				{
					Id = syllabus!.Id,
					SubjectId = syllabus.SubjectId,
					SubjectName = syllabus.Subject.Name,
					Name = syllabus.Name,
					Description = syllabus.Description,
					PdfUrl = syllabus.PdfUrl,
					CreatedAt = syllabus.CreatedAt,
					UpdatedAt = syllabus.UpdatedAt
				};

				return BaseResponse<SyllabusResponse>.Ok(response, "Syllabus created successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<SyllabusResponse>.Fail($"Error creating syllabus: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<SyllabusResponse>> UpdateSyllabusAsync(Guid id, UpdateSyllabusRequest request)
		{
			try
			{
				var syllabus = await _unitOfWork.Syllabuses.GetByIdAsync(id);

				if (syllabus == null)
				{
					return BaseResponse<SyllabusResponse>.Fail("Syllabus not found", ResponseErrorType.NotFound);
				}

				// Check if subject exists
				var subject = await _unitOfWork.Subjects.GetByIdAsync(request.SubjectId);
				if (subject == null)
				{
					return BaseResponse<SyllabusResponse>.Fail("Subject not found", ResponseErrorType.NotFound);
				}

				// Check if changing subject and new subject already has syllabus
				if (syllabus.SubjectId != request.SubjectId)
				{
					var exists = await _unitOfWork.Syllabuses.ExistsBySubjectIdAsync(request.SubjectId, id);
					if (exists)
					{
						return BaseResponse<SyllabusResponse>.Fail("A syllabus already exists for this subject", ResponseErrorType.Conflict);
					}

					// Update old subject to remove syllabus reference
					var oldSubject = await _unitOfWork.Subjects.GetByIdAsync(syllabus.SubjectId);
					if (oldSubject != null)
					{
						oldSubject.SyllabusId = Guid.Empty;
						_unitOfWork.Subjects.Update(oldSubject);
					}

					// Update new subject with syllabus ID
					subject.SyllabusId = id;
					_unitOfWork.Subjects.Update(subject);
				}

				syllabus.SubjectId = request.SubjectId;
				syllabus.Name = request.Name;
				syllabus.Description = request.Description;
				syllabus.PdfUrl = request.PdfUrl;

				_unitOfWork.Syllabuses.Update(syllabus);
				await _unitOfWork.SaveChangesAsync();

				// Reload with navigation properties
				syllabus = await _unitOfWork.Syllabuses.GetByConditionAsync(
					s => s.Id == id,
					include: q => q.Include(s => s.Subject)
				);

				var response = new SyllabusResponse
				{
					Id = syllabus!.Id,
					SubjectId = syllabus.SubjectId,
					SubjectName = syllabus.Subject.Name,
					Name = syllabus.Name,
					Description = syllabus.Description,
					PdfUrl = syllabus.PdfUrl,
					CreatedAt = syllabus.CreatedAt,
					UpdatedAt = syllabus.UpdatedAt
				};

				return BaseResponse<SyllabusResponse>.Ok(response, "Syllabus updated successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<SyllabusResponse>.Fail($"Error updating syllabus: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse> DeleteSyllabusAsync(Guid id)
		{
			try
			{
				var syllabus = await _unitOfWork.Syllabuses.GetByIdAsync(id);

				if (syllabus == null)
				{
					return BaseResponse.Fail("Syllabus not found", ResponseErrorType.NotFound);
				}

				// Update subject to remove syllabus reference
				var subject = await _unitOfWork.Subjects.GetByIdAsync(syllabus.SubjectId);
				if (subject != null)
				{
					subject.SyllabusId = Guid.Empty;
					_unitOfWork.Subjects.Update(subject);
				}

				_unitOfWork.Syllabuses.Remove(syllabus);
				await _unitOfWork.SaveChangesAsync();

				return BaseResponse.Ok("Syllabus deleted successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse.Fail($"Error deleting syllabus: {ex.Message}", ResponseErrorType.InternalError);
			}
		}
	}
}
