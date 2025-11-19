using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using giaoanpro_backend.Application.DTOs.Requests.Exams;
using giaoanpro_backend.Application.DTOs.Responses.Exams;
using giaoanpro_backend.Application.DTOs.Responses.Questions;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Services
{
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExamRepository _examRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly IMapper _mapper;

        public ExamService(IUnitOfWork unitOfWork, IExamRepository examRepository, IQuestionRepository questionRepository, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _examRepository = examRepository;
            _questionRepository = questionRepository;
            _mapper = mapper;
        }

        public async Task<BaseResponse<string>> CreateExamAsync(CreateExamRequest request, Guid teacherId)
        {
            if (request == null)
                return BaseResponse<string>.Fail("Request cannot be null.", ResponseErrorType.BadRequest);

            var now = DateTime.UtcNow;

            var exam = new Exam
            {
                Id = Guid.NewGuid(),
                CreatorId = teacherId,
                Title = request.Title,
                Description = request.Description,
                DurationMinutes = request.DurationMinutes,
                ActivityId = null,
                CreatedAt = now,
                UpdatedAt = now
            };

            var seq = 1;
            foreach (var qid in request.QuestionIds ?? Enumerable.Empty<Guid>())
            {
                var eq = new ExamQuestion
                {
                    ExamId = exam.Id,
                    QuestionId = qid,
                    SequenceNumber = seq++,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                exam.ExamQuestions.Add(eq);
            }

            await _examRepository.AddAsync(exam);
            var saved = await _examRepository.SaveChangesAsync();
            if (!saved)
                return BaseResponse<string>.Fail("Failed to create exam.", ResponseErrorType.InternalError);

            return BaseResponse<string>.Ok(exam.Id.ToString(), "Exam created successfully.");
        }

        public async Task<BaseResponse<List<ExamSummaryResponse>>> GetTeacherExamInventoryAsync(Guid teacherId)
        {
            var (items, total) = await _examRepository.GetPagedAsync(
                filter: e => e.CreatorId == teacherId && e.ActivityId == null,
                include: q => q.Include(e => e.ExamQuestions),
                orderBy: q => q.OrderByDescending(e => e.CreatedAt),
                pageNumber: 1,
                pageSize: int.MaxValue,
                asNoTracking: true
            );

            var exams = items.ToList();

            var resp = exams.Select(e => new ExamSummaryResponse
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                DurationMinutes = e.DurationMinutes,
                ActivityId = e.ActivityId,
                QuestionCount = e.ExamQuestions?.Count ?? 0
            }).ToList();

            return BaseResponse<List<ExamSummaryResponse>>.Ok(resp, "Inventory retrieved successfully.");
        }

        public async Task<BaseResponse<GetExamsPagedResponse>> GetTeacherInventoryAsync(GetExamInventoryRequest request, Guid teacherId)
        {
            if (request == null)
                return BaseResponse<GetExamsPagedResponse>.Fail("Request cannot be null.", ResponseErrorType.BadRequest);

            // Build filter
            System.Linq.Expressions.Expression<Func<Exam, bool>>? filter = e => e.CreatorId == teacherId && e.ActivityId == null && e.DeletedAt == null;

            if (!string.IsNullOrWhiteSpace(request.SearchText))
            {
                var search = request.SearchText;
                filter = e => e.CreatorId == teacherId && e.ActivityId == null && e.DeletedAt == null && e.Title.Contains(search);
            }

            var (items, totalCount) = await _examRepository.GetPagedAsync(
                filter: filter,
                include: q => q.Include(e => e.ExamQuestions),
                orderBy: q => q.OrderByDescending(e => e.CreatedAt),
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                asNoTracking: true
            );

            var exams = items.ToList();

            var dtoItems = exams.Select(e => new ExamSummaryResponse
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                DurationMinutes = e.DurationMinutes,
                ActivityId = e.ActivityId,
                QuestionCount = e.ExamQuestions?.Count ?? 0
            }).ToList();

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var response = new GetExamsPagedResponse
            {
                Items = dtoItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < totalPages
            };

            return BaseResponse<GetExamsPagedResponse>.Ok(response, "Inventory retrieved successfully.");
        }

        public async Task<BaseResponse<GetExamDetailResponse>> GetExamByIdAsync(Guid id)
        {
            var exam = await _examRepository.GetExamWithDetailsAsync(id);
            if (exam == null)
                return BaseResponse<GetExamDetailResponse>.Fail("Exam not found.", ResponseErrorType.NotFound);

            var dto = _mapper.Map<GetExamDetailResponse>(exam);

            if (exam.ExamQuestions != null)
            {
                var orderedQuestions = exam.ExamQuestions
                    .OrderBy(eq => eq.SequenceNumber)
                    .Select(eq => _mapper.Map<GetQuestionResponse>(eq.Question))
                    .ToList();

                dto.Questions = orderedQuestions;
            }

            return BaseResponse<GetExamDetailResponse>.Ok(dto, "Exam retrieved successfully.");
        }

        public async Task<BaseResponse<string>> UpdateExamAsync(UpdateExamRequest request, Guid teacherId)
        {
            if (request == null)
                return BaseResponse<string>.Fail("Request cannot be null.", ResponseErrorType.BadRequest);

            var exam = await _examRepository.GetByConditionAsync(
                predicate: e => e.Id == request.Id,
                include: q => q.Include(x => x.ExamQuestions)
            );

            if (exam == null)
                return BaseResponse<string>.Fail("Exam not found.", ResponseErrorType.NotFound);

            if (exam.CreatorId != teacherId)
                return BaseResponse<string>.Fail("You do not have permission to update this exam.", ResponseErrorType.Forbidden);

            // Update header
            exam.Title = request.Title;
            exam.Description = request.Description;
            exam.DurationMinutes = request.DurationMinutes;
            exam.UpdatedAt = DateTime.UtcNow;

            // Remove existing exam questions
            if (exam.ExamQuestions != null && exam.ExamQuestions.Any())
            {
                // Remove each exam question from the navigation collection so EF will delete them
                var toRemove = exam.ExamQuestions.ToList();
                foreach (var r in toRemove)
                {
                    exam.ExamQuestions.Remove(r);
                }
            }

            // Add new exam questions
            var seq = 1;
            var now = DateTime.UtcNow;
            foreach (var qid in request.QuestionIds ?? Enumerable.Empty<Guid>())
            {
                var eq = new ExamQuestion
                {
                    ExamId = exam.Id,
                    QuestionId = qid,
                    SequenceNumber = seq++,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                // Attach to exam navigation
                exam.ExamQuestions.Add(eq);
            }

            _examRepository.Update(exam);
            var saved = await _examRepository.SaveChangesAsync();
            if (!saved)
                return BaseResponse<string>.Fail("Failed to update exam.", ResponseErrorType.InternalError);

            return BaseResponse<string>.Ok(null!, "Exam updated successfully.");
        }

        public async Task<BaseResponse<string>> DeleteExamAsync(Guid id, Guid teacherId)
        {
            var exam = await _examRepository.GetByConditionAsync(
                predicate: e => e.Id == id,
                include: q => q.Include(x => x.ExamQuestions)
            );

            if (exam == null)
                return BaseResponse<string>.Fail("Exam not found.", ResponseErrorType.NotFound);

            if (exam.CreatorId != teacherId)
                return BaseResponse<string>.Fail("You do not have permission to delete this exam.", ResponseErrorType.Forbidden);

            exam.DeletedAt = DateTime.UtcNow;
            _examRepository.Update(exam);

            var saved = await _examRepository.SaveChangesAsync();
            if (!saved)
                return BaseResponse<string>.Fail("Failed to delete exam.", ResponseErrorType.InternalError);

            return BaseResponse<string>.Ok(null!, "Exam deleted successfully.");
        }
    }
}
