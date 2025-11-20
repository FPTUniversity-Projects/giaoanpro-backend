using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using giaoanpro_backend.Application.DTOs.Requests.Attempts;
using giaoanpro_backend.Application.DTOs.Responses.Attempts;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.Services
{
    public class AttemptService : IAttemptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Attempt> _attemptRepo;
        private readonly IGenericRepository<AttemptDetail> _attemptDetailRepo;

        public AttemptService(IUnitOfWork unitOfWork, IGenericRepository<Attempt> attemptRepo, IGenericRepository<AttemptDetail> attemptDetailRepo)
        {
            _unitOfWork = unitOfWork;
            _attemptRepo = attemptRepo;
            _attemptDetailRepo = attemptDetailRepo;
        }

        public async Task<BaseResponse<ExamPaperResponse>> StartAttemptAsync(StartAttemptRequest request, Guid userId)
        {
            if (request == null) return BaseResponse<ExamPaperResponse>.Fail("Request cannot be null.", ResponseErrorType.BadRequest);
            if (request.ExamId == Guid.Empty) return BaseResponse<ExamPaperResponse>.Fail("Invalid exam id.", ResponseErrorType.BadRequest);

            var exam = await _unitOfWork.Exams.GetExamWithDetailsAsync(request.ExamId);
            if (exam == null) return BaseResponse<ExamPaperResponse>.Fail("Exam not found.", ResponseErrorType.NotFound);

            var existing = await _attemptRepo.Query(asNoTracking: false)
                .Include(a => a.AttemptDetails)
                .FirstOrDefaultAsync(a => a.ExamId == request.ExamId && a.UserId == userId && a.Status == AttemptStatus.InProgress);

            if (existing != null)
            {
                var start = existing.StartedAt;
                var duration = TimeSpan.FromMinutes(exam.DurationMinutes);
                if (DateTime.UtcNow > start.Add(duration))
                {
                    existing.Status = AttemptStatus.Submitted;
                    existing.CompletedAt = DateTime.UtcNow;
                    existing.UpdatedAt = DateTime.UtcNow;
                    _attemptRepo.Update(existing);
                    await _attemptRepo.SaveChangesAsync();
                    return BaseResponse<ExamPaperResponse>.Fail("Previous attempt time expired and was auto-submitted.", ResponseErrorType.BadRequest);
                }

                var paper = MapToPaper(existing, exam);
                return BaseResponse<ExamPaperResponse>.Ok(paper);
            }

            var attempt = new Attempt
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExamId = exam.Id,
                StartedAt = DateTime.UtcNow,
                Status = AttemptStatus.InProgress,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _attemptRepo.AddAsync(attempt);
            var saved = await _attemptRepo.SaveChangesAsync();
            if (!saved) return BaseResponse<ExamPaperResponse>.Fail("Failed to create attempt.", ResponseErrorType.InternalError);

            var paperResp = MapToPaper(attempt, exam);
            return BaseResponse<ExamPaperResponse>.Ok(paperResp);
        }

        public async Task<BaseResponse<string>> SaveProgressAsync(UpdateProgressRequest request, Guid userId)
        {
            if (request == null) return BaseResponse<string>.Fail("Request cannot be null.", ResponseErrorType.BadRequest);
            if (request.AttemptId == Guid.Empty) return BaseResponse<string>.Fail("Invalid attempt id.", ResponseErrorType.BadRequest);

            var attempt = await _attemptRepo.Query(asNoTracking: false)
                .Include(a => a.AttemptDetails)
                .FirstOrDefaultAsync(a => a.Id == request.AttemptId && a.UserId == userId);

            if (attempt == null) return BaseResponse<string>.Fail("Attempt not found.", ResponseErrorType.NotFound);
            if (attempt.Status != AttemptStatus.InProgress) return BaseResponse<string>.Fail("Attempt is not in progress.", ResponseErrorType.BadRequest);

            var exam = await _unitOfWork.Exams.GetByIdAsync(attempt.ExamId);
            if (exam == null) return BaseResponse<string>.Fail("Exam not found.", ResponseErrorType.NotFound);

            var start = attempt.StartedAt;
            var duration = TimeSpan.FromMinutes(exam.DurationMinutes);
            if (DateTime.UtcNow > start.Add(duration)) return BaseResponse<string>.Fail("Time Expired.", ResponseErrorType.BadRequest);

            foreach (var ans in request.Answers)
            {
                var detail = attempt.AttemptDetails?.FirstOrDefault(d => d.QuestionId == ans.QuestionId);
                if (detail != null)
                {
                    detail.Answer = ans.TextAnswer ?? string.Empty;
                    if (ans.SelectedOptionId.HasValue)
                        detail.Answer = ans.SelectedOptionId.Value.ToString();
                    detail.UpdatedAt = DateTime.UtcNow;
                    _attemptDetailRepo.Update(detail);
                }
                else
                {
                    var newDet = new AttemptDetail
                    {
                        Id = Guid.NewGuid(),
                        AttemptId = attempt.Id,
                        QuestionId = ans.QuestionId,
                        Answer = ans.SelectedOptionId.HasValue ? ans.SelectedOptionId.Value.ToString() : (ans.TextAnswer ?? string.Empty),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _attemptDetailRepo.AddAsync(newDet);
                }
            }

            var saved = await _attemptRepo.SaveChangesAsync();
            if (!saved) return BaseResponse<string>.Fail("Failed to save progress.", ResponseErrorType.InternalError);

            return BaseResponse<string>.Ok("Progress saved.");
        }

        public async Task<BaseResponse<AttemptResultResponse>> SubmitAttemptAsync(SubmitAttemptRequest request, Guid userId)
        {
            if (request == null) return BaseResponse<AttemptResultResponse>.Fail("Request cannot be null.", ResponseErrorType.BadRequest);
            if (request.AttemptId == Guid.Empty) return BaseResponse<AttemptResultResponse>.Fail("Invalid attempt id.", ResponseErrorType.BadRequest);

            var attempt = await _attemptRepo.Query(asNoTracking: false)
                .Include(a => a.AttemptDetails)
                .FirstOrDefaultAsync(a => a.Id == request.AttemptId && a.UserId == userId);

            if (attempt == null) return BaseResponse<AttemptResultResponse>.Fail("Attempt not found.", ResponseErrorType.NotFound);
            if (attempt.Status != AttemptStatus.InProgress) return BaseResponse<AttemptResultResponse>.Fail("Attempt cannot be submitted in its current state.", ResponseErrorType.BadRequest);

            var exam = await _unitOfWork.Exams.GetExamWithDetailsAsync(attempt.ExamId);
            if (exam == null) return BaseResponse<AttemptResultResponse>.Fail("Associated exam not found.", ResponseErrorType.NotFound);

            var start = attempt.StartedAt;
            var duration = TimeSpan.FromMinutes(exam.DurationMinutes);
            var buffer = TimeSpan.FromMinutes(1);
            if (DateTime.UtcNow > start.Add(duration).Add(buffer))
            {
                attempt.Status = AttemptStatus.Submitted;
                attempt.CompletedAt = DateTime.UtcNow;
                attempt.UpdatedAt = DateTime.UtcNow;
                _attemptRepo.Update(attempt);
                await _attemptRepo.SaveChangesAsync();
                return BaseResponse<AttemptResultResponse>.Fail("Time window exceeded. Submission rejected.", ResponseErrorType.BadRequest);
            }

            decimal autoScore = 0m;
            bool hasExercise = false;

            var answerLookup = request.Answers?.ToDictionary(a => a.QuestionId, a => a) ?? new Dictionary<Guid, StudentAnswerRequest>();

            foreach (var eq in exam.ExamQuestions)
            {
                var q = eq.Question;
                if (q == null) continue;

                if (q.QuestionType == QuestionType.Theory)
                {
                    var detail = attempt.AttemptDetails?.FirstOrDefault(d => d.QuestionId == q.Id);
                    Guid? selectedOptionId = null;
                    if (answerLookup.TryGetValue(q.Id, out var provided))
                    {
                        selectedOptionId = provided.SelectedOptionId;
                    }
                    else if (detail != null && Guid.TryParse(detail.Answer, out var parsed))
                    {
                        selectedOptionId = parsed;
                    }

                    if (selectedOptionId.HasValue)
                    {
                        var opt = q.Options.FirstOrDefault(o => o.Id == selectedOptionId.Value);
                        if (opt != null && opt.IsCorrect)
                        {
                            autoScore += 1m;
                            if (detail != null)
                            {
                                detail.IsCorrect = true;
                                detail.Score = 1;
                                detail.UpdatedAt = DateTime.UtcNow;
                                _attemptDetailRepo.Update(detail);
                            }
                        }
                        else
                        {
                            if (detail != null)
                            {
                                detail.IsCorrect = false;
                                detail.Score = 0;
                                detail.UpdatedAt = DateTime.UtcNow;
                                _attemptDetailRepo.Update(detail);
                            }
                        }
                    }
                }
                else if (q.QuestionType == QuestionType.Exercise)
                {
                    hasExercise = true;
                    if (answerLookup.TryGetValue(q.Id, out var provided))
                    {
                        var detail = attempt.AttemptDetails?.FirstOrDefault(d => d.QuestionId == q.Id);
                        if (detail != null)
                        {
                            detail.Answer = provided.TextAnswer ?? string.Empty;
                            detail.UpdatedAt = DateTime.UtcNow;
                            _attemptDetailRepo.Update(detail);
                        }
                        else
                        {
                            var newDet = new AttemptDetail
                            {
                                Id = Guid.NewGuid(),
                                AttemptId = attempt.Id,
                                QuestionId = q.Id,
                                Answer = provided.TextAnswer ?? string.Empty,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            await _attemptDetailRepo.AddAsync(newDet);
                        }
                    }
                }
            }

            attempt.AutoScore = autoScore;
            attempt.ManualScore = attempt.ManualScore;
            attempt.FinalScore = (int)autoScore;
            attempt.CompletedAt = DateTime.UtcNow;
            attempt.Status = hasExercise ? AttemptStatus.PendingGrading : AttemptStatus.Completed;
            attempt.UpdatedAt = DateTime.UtcNow;

            _attemptRepo.Update(attempt);
            var saved = await _attemptRepo.SaveChangesAsync();
            if (!saved) return BaseResponse<AttemptResultResponse>.Fail("Failed to submit attempt.", ResponseErrorType.InternalError);

            var result = new AttemptResultResponse
            {
                AttemptId = attempt.Id,
                AutoScore = attempt.AutoScore ?? 0m,
                ManualScore = attempt.ManualScore,
                TotalScore = (attempt.AutoScore ?? 0m) + (attempt.ManualScore ?? 0m),
                Status = attempt.Status.ToString()
            };

            return BaseResponse<AttemptResultResponse>.Ok(result, "Attempt submitted successfully.");
        }

        public async Task<BaseResponse<GetPendingAttemptsResponse>> GetPendingAttemptsAsync(Guid teacherId)
        {
            var attempts = await _attemptRepo.GetAllAsync(
                filter: a => a.Status == AttemptStatus.PendingGrading,
                include: q => q.Include(a => a.Exam).ThenInclude(e => e.Creator).Include(a => a.User),
                asNoTracking: true
            );

            var list = attempts
                .Where(a => a.Exam.CreatorId == teacherId)
                .Select(a => new PendingAttemptDto
                {
                    AttemptId = a.Id,
                    StudentId = a.UserId,
                    StudentName = a.User.FullName,
                    ExamId = a.ExamId,
                    ExamTitle = a.Exam.Title,
                    StartedAt = a.StartedAt,
                    CompletedAt = a.CompletedAt
                }).ToList();

            var resp = new GetPendingAttemptsResponse { Items = list };
            return BaseResponse<GetPendingAttemptsResponse>.Ok(resp);
        }

        public async Task<BaseResponse<string>> GradeAttemptAsync(GradeAttemptRequest request, Guid teacherId)
        {
            if (request == null) return BaseResponse<string>.Fail("Request cannot be null.", ResponseErrorType.BadRequest);
            if (request.AttemptId == Guid.Empty) return BaseResponse<string>.Fail("Invalid attempt id.", ResponseErrorType.BadRequest);

            var attempt = await _attemptRepo.Query(asNoTracking: false)
                .Include(a => a.AttemptDetails).ThenInclude(d => d.Question)
                .Include(a => a.Exam)
                .FirstOrDefaultAsync(a => a.Id == request.AttemptId);

            if (attempt == null) return BaseResponse<string>.Fail("Attempt not found.", ResponseErrorType.NotFound);

            if (attempt.Exam.CreatorId != teacherId)
                return BaseResponse<string>.Fail("You do not have permission to grade this attempt.", ResponseErrorType.Forbidden);

            decimal manualScore = 0m;

            foreach (var g in request.Grades)
            {
                var detail = attempt.AttemptDetails?.FirstOrDefault(d => d.QuestionId == g.QuestionId);
                if (detail == null)
                {
                    detail = new AttemptDetail
                    {
                        Id = Guid.NewGuid(),
                        AttemptId = attempt.Id,
                        QuestionId = g.QuestionId,
                        Grade = g.Score,
                        TeacherFeedback = g.Feedback ?? string.Empty,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _attemptDetailRepo.AddAsync(detail);
                }
                else
                {
                    detail.Grade = g.Score;
                    detail.TeacherFeedback = g.Feedback ?? string.Empty;
                    detail.UpdatedAt = DateTime.UtcNow;
                    _attemptDetailRepo.Update(detail);
                }

                manualScore += g.Score;
            }

            attempt.ManualScore = manualScore;
            attempt.AutoScore = attempt.AutoScore ?? 0m;
            attempt.FinalScore = (int)Math.Round((double)((attempt.AutoScore ?? 0m) + (attempt.ManualScore ?? 0m)));
            attempt.Status = AttemptStatus.Completed;
            attempt.UpdatedAt = DateTime.UtcNow;

            _attemptRepo.Update(attempt);
            var saved = await _attemptRepo.SaveChangesAsync();
            if (!saved) return BaseResponse<string>.Fail("Failed to save grading result.", ResponseErrorType.InternalError);

            return BaseResponse<string>.Ok("Attempt graded successfully.");
        }

        public async Task<BaseResponse<List<AttemptSummaryResponse>>> GetMyAttemptsAsync(Guid studentId)
        {
            if (studentId == Guid.Empty) return BaseResponse<List<AttemptSummaryResponse>>.Fail("Invalid student id.", ResponseErrorType.BadRequest);

            var attempts = await _attemptRepo.GetAllAsync(
                filter: a => a.UserId == studentId,
                include: q => q.Include(a => a.Exam),
                orderBy: q => q.OrderByDescending(a => a.StartedAt),
                asNoTracking: true
            );

            var list = attempts.Select(a => new AttemptSummaryResponse
            {
                AttemptId = a.Id,
                ExamTitle = a.Exam?.Title ?? string.Empty,
                StartTime = a.StartedAt,
                EndTime = a.CompletedAt,
                Status = a.Status.ToString(),
                TotalScore = (double?)((a.AutoScore ?? 0m) + (a.ManualScore ?? 0m)),
                Grade = null
            }).ToList();

            return BaseResponse<List<AttemptSummaryResponse>>.Ok(list);
        }

        public async Task<BaseResponse<List<AttemptSummaryResponse>>> GetAttemptsByExamAsync(Guid examId, Guid teacherId)
        {
            if (examId == Guid.Empty) return BaseResponse<List<AttemptSummaryResponse>>.Fail("Invalid exam id.", ResponseErrorType.BadRequest);

            var exam = await _unitOfWork.Exams.GetByIdAsync(examId);
            if (exam == null) return BaseResponse<List<AttemptSummaryResponse>>.Fail("Exam not found.", ResponseErrorType.NotFound);
            if (exam.CreatorId != teacherId) return BaseResponse<List<AttemptSummaryResponse>>.Fail("You do not have permission.", ResponseErrorType.Forbidden);

            var attempts = await _attemptRepo.GetAllAsync(
                filter: a => a.ExamId == examId,
                include: q => q.Include(a => a.User).Include(a => a.Exam),
                orderBy: q => q.OrderByDescending(a => a.StartedAt),
                asNoTracking: true
            );

            var list = attempts.Select(a => new AttemptSummaryResponse
            {
                AttemptId = a.Id,
                ExamTitle = a.Exam?.Title ?? string.Empty,
                StartTime = a.StartedAt,
                EndTime = a.CompletedAt,
                Status = a.Status.ToString(),
                TotalScore = (double?)((a.AutoScore ?? 0m) + (a.ManualScore ?? 0m)),
                Grade = null
            }).ToList();

            return BaseResponse<List<AttemptSummaryResponse>>.Ok(list);
        }

        public async Task<BaseResponse<AttemptReviewResponse>> GetAttemptDetailAsync(Guid attemptId, Guid currentUserId)
        {
            if (attemptId == Guid.Empty) return BaseResponse<AttemptReviewResponse>.Fail("Invalid attempt id.", ResponseErrorType.BadRequest);

            var attempt = await _attemptRepo.Query(asNoTracking: true)
                .Include(a => a.AttemptDetails).ThenInclude(d => d.Question).ThenInclude(q => q.Options)
                .Include(a => a.Exam).ThenInclude(e => e.ExamQuestions).ThenInclude(eq => eq.Question).ThenInclude(q => q.Options)
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return BaseResponse<AttemptReviewResponse>.Fail("Attempt not found.", ResponseErrorType.NotFound);

            var isStudent = attempt.UserId == currentUserId;
            var isTeacher = attempt.Exam.CreatorId == currentUserId;
            if (!isStudent && !isTeacher) return BaseResponse<AttemptReviewResponse>.Fail("Access denied.", ResponseErrorType.Forbidden);

            var response = new AttemptReviewResponse
            {
                AttemptId = attempt.Id,
                ExamTitle = attempt.Exam?.Title ?? string.Empty,
                StartTime = attempt.StartedAt,
                EndTime = attempt.CompletedAt,
                Status = attempt.Status.ToString(),
                TotalScore = (double?)((attempt.AutoScore ?? 0m) + (attempt.ManualScore ?? 0m)),
                Grade = null
            };

            var detailMap = attempt.AttemptDetails?.ToDictionary(d => d.QuestionId) ?? new Dictionary<Guid, AttemptDetail>();

            var orderedQuestions = attempt.Exam?.ExamQuestions?.OrderBy(eq => eq.SequenceNumber).Select(eq => eq.Question).ToList() ?? new List<Question>();

            foreach (var q in orderedQuestions)
            {
                var detail = detailMap.TryGetValue(q.Id, out var d) ? d : null;

                var dto = new AttemptDetailReviewDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.Text,
                    QuestionType = q.QuestionType.ToString(),
                    Options = q.Options?.Select(o => new OptionReviewDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = (attempt.Status == AttemptStatus.Completed || isTeacher) ? (bool?)o.IsCorrect : null
                    }).ToList() ?? new List<OptionReviewDto>(),
                    StudentAnswer = detail?.Answer,
                    IsCorrect = detail?.IsCorrect,
                    Score = detail?.Grade ?? (detail != null ? (decimal?)detail.Score : null),
                    TeacherFeedback = detail?.TeacherFeedback
                };

                if (attempt.Status != AttemptStatus.Completed && isStudent)
                {
                    foreach (var opt in dto.Options)
                    {
                        opt.IsCorrect = null;
                    }

                    dto.IsCorrect = null;
                    dto.Score = null;
                }

                response.Details.Add(dto);
            }

            return BaseResponse<AttemptReviewResponse>.Ok(response);
        }

        // Helper mapping - strip correct flags
        private ExamPaperResponse MapToPaper(Attempt attempt, Exam exam)
        {
            var resp = new ExamPaperResponse
            {
                AttemptId = attempt.Id,
                ExamTitle = exam.Title,
                DurationMinutes = exam.DurationMinutes,
                StartTime = attempt.StartedAt,
                Questions = new List<QuestionPaperDto>()
            };

            if (exam.ExamQuestions != null)
            {
                foreach (var eq in exam.ExamQuestions.OrderBy(eq => eq.SequenceNumber))
                {
                    var q = eq.Question;
                    var qp = new QuestionPaperDto
                    {
                        Id = q.Id,
                        Text = q.Text,
                        QuestionType = q.QuestionType.ToString(),
                        Options = q.Options?.Select(o => new OptionPaperDto { Id = o.Id, Text = o.Text }).ToList() ?? new List<OptionPaperDto>()
                    };
                    resp.Questions.Add(qp);
                }
            }

            return resp;
        }
    }
}
