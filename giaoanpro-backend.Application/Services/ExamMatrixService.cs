using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using giaoanpro_backend.Application.DTOs.Requests.ExamMatrices;
using giaoanpro_backend.Application.DTOs.Responses.ExamMatrices;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Application.Services
{
    public class ExamMatrixService : IExamMatrixService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExamMatrixService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<BaseResponse<GetMatrixResponse>> CreateMatrixAsync(CreateMatrixRequest request)
        {
            if (request == null)
                return BaseResponse<GetMatrixResponse>.Fail("Request cannot be null.", ResponseErrorType.BadRequest);

            // Validate subject exists
            var subject = await _unitOfWork.Subjects.GetByIdAsync(request.SubjectId);
            if (subject == null)
                return BaseResponse<GetMatrixResponse>.Fail("Subject not found.", ResponseErrorType.BadRequest);

            // Validate lesson plan exists
            var lessonPlan = await _unitOfWork.LessonPlans.GetByIdAsync(request.LessonPlanId);
            if (lessonPlan == null)
                return BaseResponse<GetMatrixResponse>.Fail("LessonPlan not found.", ResponseErrorType.BadRequest);

            var now = DateTime.UtcNow;
            var matrix = new ExamMatrix
            {
                Id = Guid.NewGuid(),
                SubjectId = request.SubjectId,
                LessonPlanId = request.LessonPlanId,
                Name = request.Name,
                TotalQuestions = request.Details?.Sum(l => l.Count) ?? 0,
                CreatedAt = now,
                UpdatedAt = now
            };

            // Add lines
            foreach (var line in request.Details ?? Enumerable.Empty<MatrixDetailRequest>())
            {
                var ml = new ExamMatrixDetail
                {
                    Id = Guid.NewGuid(),
                    MatrixId = matrix.Id,
                    QuestionType = line.QuestionType,
                    DifficultyLevel = line.DifficultyLevel,
                    Count = line.Count,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                matrix.Lines.Add(ml);
            }

            // Persist via repository
            await _unitOfWork.ExamMatrices.AddAsync(matrix);

            var saved = await _unitOfWork.SaveChangesAsync();
            if (!saved)
                return BaseResponse<GetMatrixResponse>.Fail("Failed to create matrix.", ResponseErrorType.InternalError);

            var resp = new GetMatrixResponse
            {
                Id = matrix.Id,
                Name = matrix.Name,
                SubjectId = matrix.SubjectId,
                Lines = matrix.Lines.Select(l => new MatrixDetailResponse { Id = l.Id, QuestionType = l.QuestionType, DifficultyLevel = l.DifficultyLevel, Count = l.Count }).ToList()
            };

            return BaseResponse<GetMatrixResponse>.Ok(resp, "Matrix created successfully.");
        }

        public async Task<BaseResponse<GetMatrixDetailResponse>> GetMatrixByIdAsync(Guid id)
        {
            var matrix = await _unitOfWork.ExamMatrices.GetByConditionAsync(
                em => em.Id == id,
                include: q => q.Include(em => em.Lines),
                asNoTracking: true
            );

            if (matrix == null)
                return BaseResponse<GetMatrixDetailResponse>.Fail("Matrix not found.", ResponseErrorType.NotFound);

            var dto = new GetMatrixDetailResponse
            {
                Id = matrix.Id,
                Name = matrix.Name,
                SubjectId = matrix.SubjectId,
                TotalQuestions = matrix.TotalQuestions,
                DurationMinutes = 0, // not present on entity; default 0
                Lines = matrix.Lines.Select(l => new GetMatrixLineResponse { Id = l.Id, QuestionType = l.QuestionType, DifficultyLevel = l.DifficultyLevel, Count = l.Count, PointsPerQuestion = 0 }).ToList()
            };

            return BaseResponse<GetMatrixDetailResponse>.Ok(dto, "Matrix retrieved successfully.");
        }

        public async Task<BaseResponse<GetMatrixPagedResponse>> GetMatricesPagedAsync(GetMatrixPagedRequest request)
        {
            if (request == null)
                return BaseResponse<GetMatrixPagedResponse>.Fail("Request cannot be null.", ResponseErrorType.BadRequest);

            System.Linq.Expressions.Expression<Func<ExamMatrix, bool>>? filter = em => em.DeletedAt == null;

            if (request.SubjectId.HasValue || !string.IsNullOrWhiteSpace(request.SearchText))
            {
                var sid = request.SubjectId;
                var search = request.SearchText?.Trim();
                filter = em => em.DeletedAt == null
                               && (!sid.HasValue || em.SubjectId == sid.Value)
                               && (string.IsNullOrWhiteSpace(search) || em.Name.Contains(search));
            }

            var (items, total) = await _unitOfWork.ExamMatrices.GetPagedAsync(
                filter: filter,
                include: q => q.Include(em => em.Subject),
                orderBy: q => q.OrderByDescending(em => em.CreatedAt),
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                asNoTracking: true
            );

            var list = items.Select(em => new ExamMatrixSummaryDto
            {
                Id = em.Id,
                Name = em.Name,
                SubjectName = em.Subject?.Name ?? string.Empty,
                TotalQuestions = em.TotalQuestions,
                Duration = 0
            }).ToList();

            var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

            var resp = new GetMatrixPagedResponse
            {
                Items = list,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = total,
                TotalPages = totalPages,
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < totalPages
            };

            return BaseResponse<GetMatrixPagedResponse>.Ok(resp, "Matrices retrieved successfully.");
        }

        public async Task<BaseResponse<string>> UpdateMatrixAsync(UpdateMatrixRequest request)
        {
            if (request == null)
                return BaseResponse<string>.Fail("Request cannot be null.", ResponseErrorType.BadRequest);

            var matrix = await _unitOfWork.ExamMatrices.GetByConditionAsync(
                em => em.Id == request.Id,
                include: q => q.Include(em => em.Lines)
            );

            if (matrix == null)
                return BaseResponse<string>.Fail("Matrix not found.", ResponseErrorType.NotFound);

            // Update header
            matrix.Name = request.Name;
            matrix.TotalQuestions = request.TotalQuestions;
            matrix.UpdatedAt = DateTime.UtcNow;

            // Remove existing details by clearing the tracked collection
            if (matrix.Lines != null && matrix.Lines.Any())
            {
                // detach existing entries by removing them from the collection so EF will delete them
                var toRemove = matrix.Lines.ToList();
                foreach (var rem in toRemove)
                {
                    matrix.Lines.Remove(rem);
                }
            }

            // Map and add new details to the tracked entity
            var now = DateTime.UtcNow;
            foreach (var d in request.Details)
            {
                var detail = new ExamMatrixDetail
                {
                    Id = Guid.NewGuid(),
                    MatrixId = matrix.Id,
                    QuestionType = d.QuestionType,
                    DifficultyLevel = d.DifficultyLevel,
                    Count = d.Count,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                matrix.Lines.Add(detail);
            }

            _unitOfWork.ExamMatrices.Update(matrix);

            var saved = await _unitOfWork.SaveChangesAsync();
            if (!saved)
                return BaseResponse<string>.Fail("Failed to update matrix.", ResponseErrorType.InternalError);

            return BaseResponse<string>.Ok(null!, "Matrix updated successfully.");
        }

        public async Task<BaseResponse<string>> DeleteMatrixAsync(Guid id)
        {
            var matrix = await _unitOfWork.ExamMatrices.GetByIdAsync(id);
            if (matrix == null)
                return BaseResponse<string>.Fail("Matrix not found.", ResponseErrorType.NotFound);

            // Check usage
            var used = await _unitOfWork.Exams.AnyAsync(e => e.MatrixId == id);
            if (used)
                return BaseResponse<string>.Fail("Cannot delete Matrix used by Exams", ResponseErrorType.Conflict);

            matrix.DeletedAt = DateTime.UtcNow;
            _unitOfWork.ExamMatrices.Update(matrix);

            var saved = await _unitOfWork.SaveChangesAsync();
            if (!saved)
                return BaseResponse<string>.Fail("Failed to delete matrix.", ResponseErrorType.InternalError);

            return BaseResponse<string>.Ok(null!, "Matrix deleted successfully.");
        }
    }
}
