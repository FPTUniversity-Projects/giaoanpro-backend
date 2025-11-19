using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Questions;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Questions;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;
using giaoanpro_backend.Domain.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq.Expressions;
using System.Text.Json;

namespace giaoanpro_backend.Application.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IGenericRepository<Question> _questionRepository;
        private readonly IGenericRepository<QuestionOption> _optionRepository;
        private readonly IGenericRepository<LessonPlan> _lessonRepository;
        private readonly IMapper _mapper;
        private readonly IGeminiService _gemini;

        public QuestionService(
            IGenericRepository<Question> questionRepository,
            IGenericRepository<QuestionOption> optionRepository,
            IGenericRepository<LessonPlan> lessonRepository,

            IMapper mapper,
            IGeminiService gemini)
        {
            _questionRepository = questionRepository;
            _optionRepository = optionRepository;
            _lessonRepository = lessonRepository;
            _mapper = mapper;
            _gemini = gemini;
        }

        public async Task<BaseResponse<List<GetQuestionResponse>>> GenerateQuestionsAiAsync(GenerateQuestionsRequest request)
        {
            // Validate lesson plan exists
            var lesson = await _lessonRepository.GetByConditionAsync(
                lp => lp.Id == request.LessonPlanId,
                include: q => q.Include(x => x.Activities),
                asNoTracking: true
            );
            if (lesson == null)
                return BaseResponse<List<GetQuestionResponse>>.Fail("LessonPlan not found.");

            // Build literature-focused context from LessonPlan + Activities
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Tiêu đề: {lesson.Title}");
            if (!string.IsNullOrWhiteSpace(lesson.Objective)) sb.AppendLine($"Mục tiêu: {lesson.Objective}");
            if (!string.IsNullOrWhiteSpace(lesson.Note)) sb.AppendLine($"Ghi chú: {lesson.Note}");
            if (lesson.Activities != null && lesson.Activities.Any())
            {
                sb.AppendLine("Các hoạt động:");
                foreach (var act in lesson.Activities)
                {
                    if (!string.IsNullOrWhiteSpace(act.Title))
                    {
                        sb.AppendLine($"- {act.Title}");
                    }
                }
            }

            var context = sb.ToString();
            var createdIds = new List<Guid>();

            foreach (var spec in request.Specs)
            {
                var count = Math.Clamp(spec.Count, 1, 10);

                string json;
                try
                {
                    json = await _gemini.GenerateQuestionsJsonAsync(request.LessonPlanId, context, spec, count);
                }
                catch (Exception ex)
                {
                    return BaseResponse<List<GetQuestionResponse>>.Fail($"AI generation failed: {ex.Message}");
                }
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (!doc.RootElement.TryGetProperty("questions", out var questionsEl) || questionsEl.ValueKind != JsonValueKind.Array)
                    {
                        return BaseResponse<List<GetQuestionResponse>>.Fail("AI returned invalid JSON schema.");
                    }

                    int i = 0;
                    foreach (var qEl in questionsEl.EnumerateArray())
                    {
                        if (i++ >= count) break;
                        var text = qEl.GetProperty("text").GetString() ?? string.Empty;

                        var q = new Question
                        {
                            Id = Guid.NewGuid(),
                            LessonPlanId = request.LessonPlanId,
                            Text = text,
                            QuestionType = spec.QuestionType,
                            DifficultyLevel = spec.DifficultyLevel,
                            AwarenessLevel = spec.AwarenessLevel,
                            DeletedAt = null,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _questionRepository.AddAsync(q);
                        createdIds.Add(q.Id);

                        if (qEl.TryGetProperty("options", out var optsEl) && optsEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var oEl in optsEl.EnumerateArray())
                            {
                                var optText = oEl.GetProperty("text").GetString() ?? string.Empty;
                                var isCorrect = oEl.GetProperty("isCorrect").GetBoolean();
                                var opt = new QuestionOption
                                {
                                    Id = Guid.NewGuid(),
                                    QuestionId = q.Id,
                                    Text = optText,
                                    IsCorrect = isCorrect,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                await _optionRepository.AddAsync(opt);
                            }
                        }
                        else
                        {
                            // Fallback to 2 options
                            await _optionRepository.AddAsync(new QuestionOption
                            {
                                Id = Guid.NewGuid(), QuestionId = q.Id, Text = "Correct", IsCorrect = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                            });
                            await _optionRepository.AddAsync(new QuestionOption
                            {
                                Id = Guid.NewGuid(), QuestionId = q.Id, Text = "Wrong", IsCorrect = false, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
                catch
                {
                    return BaseResponse<List<GetQuestionResponse>>.Fail("Failed to parse AI response.");
                }
            }

            var saved = await _questionRepository.SaveChangesAsync();
            if (!saved)
            {
                return BaseResponse<List<GetQuestionResponse>>.Fail("Failed to persist generated questions.");
            }

            var loaded = await _questionRepository.GetAllAsync(
                filter: q => createdIds.Contains(q.Id),
                include: q => q.Include(x => x.Options),
                asNoTracking: true);
            var responses = _mapper.Map<List<GetQuestionResponse>>(loaded.ToList());
            return BaseResponse<List<GetQuestionResponse>>.Ok(responses, "AI questions generated successfully.");
        }

        public async Task<BaseResponse<GetQuestionsPagedResponse>> GetAllQuestionsAsync(GetQuestionsRequest request)
        {
            // Build filter expression (always exclude soft-deleted)
            Expression<Func<Question, bool>>? filter = null;

            if (request.QuestionType.HasValue || request.DifficultyLevel.HasValue || request.AwarenessLevel.HasValue || request.LessonPlanId.HasValue || !string.IsNullOrWhiteSpace(request.SearchText))
            {
                filter = q =>
                    q.DeletedAt == null &&
                    (!request.QuestionType.HasValue || q.QuestionType == request.QuestionType.Value) &&
                    (!request.DifficultyLevel.HasValue || q.DifficultyLevel == request.DifficultyLevel.Value) &&
                    (!request.AwarenessLevel.HasValue || q.AwarenessLevel == request.AwarenessLevel.Value) &&
                    (!request.LessonPlanId.HasValue || q.LessonPlanId == request.LessonPlanId.Value) &&
                    (string.IsNullOrWhiteSpace(request.SearchText) || q.Text.Contains(request.SearchText));
            }
            else
            {
                filter = q => q.DeletedAt == null;
            }

            var (questions, totalCount) = await _questionRepository.GetPagedAsync(
                filter: filter,
                include: q => q.Include(x => x.Options),
                orderBy: q => q.OrderByDescending(x => x.CreatedAt),
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                asNoTracking: true
            );

            var items = _mapper.Map<List<GetQuestionResponse>>(questions);
            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var response = new GetQuestionsPagedResponse
            {
                Items = items,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                HasPreviousPage = request.PageNumber > 1,
                HasNextPage = request.PageNumber < totalPages
            };

            return BaseResponse<GetQuestionsPagedResponse>.Ok(response, "Questions retrieved successfully.");
        }

        public async Task<BaseResponse<GetQuestionResponse>> GetQuestionByIdAsync(Guid id)
        {
            var question = await _questionRepository.GetByConditionAsync(
                predicate: q => q.Id == id && q.DeletedAt == null,
                include: q => q.Include(x => x.Options).Include(x => x.LessonPlan),
                asNoTracking: true
            );

            if (question == null)
            {
                return BaseResponse<GetQuestionResponse>.Fail("Question not found.");
            }

            var response = _mapper.Map<GetQuestionResponse>(question);
            return BaseResponse<GetQuestionResponse>.Ok(response, "Question retrieved successfully.");
        }

        public async Task<BaseResponse<string>> CreateQuestionAsync(CreateQuestionRequest request)
        {
            // Validate LessonPlan existence
            var hasLesson = await _lessonRepository.AnyAsync(lp => lp.Id == request.LessonPlanId);
            if (!hasLesson)
            {
                return BaseResponse<string>.Fail("LessonPlan not found.");
            }

            // Create question
            var question = new Question
            {
                Id = Guid.NewGuid(),
                Text = request.Text,
                QuestionType = request.QuestionType,
                DifficultyLevel = request.DifficultyLevel,
                AwarenessLevel = request.AwarenessLevel,
                LessonPlanId = request.LessonPlanId,
                DeletedAt = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _questionRepository.AddAsync(question);

            // Create options
            foreach (var optionDto in request.Options)
            {
                var option = new QuestionOption
                {
                    Id = Guid.NewGuid(),
                    QuestionId = question.Id,
                    Text = optionDto.Text,
                    IsCorrect = optionDto.IsCorrect,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _optionRepository.AddAsync(option);
            }

            var result = await _questionRepository.SaveChangesAsync();
            return result
                ? BaseResponse<string>.Ok(question.Id.ToString(), "Question created successfully.")
                : BaseResponse<string>.Fail("Failed to create question.");
        }

        public async Task<BaseResponse<string>> UpdateQuestionAsync(Guid id, UpdateQuestionRequest request)
        {
            var question = await _questionRepository.GetByConditionAsync(
                predicate: q => q.Id == id && q.DeletedAt == null,
                include: q => q.Include(x => x.Options)
            );

            if (question == null)
            {
                return BaseResponse<string>.Fail("Question not found.");
            }

            // Update question text
            question.Text = request.Text;
            question.QuestionType = request.QuestionType;
            question.DifficultyLevel = request.DifficultyLevel;
            question.AwarenessLevel = request.AwarenessLevel;
            question.UpdatedAt = DateTime.UtcNow;
            _questionRepository.Update(question);

            // Update options
            var existingOptionIds = question.Options.Select(o => o.Id).ToList();
            var requestOptionIds = request.Options.Where(o => o.Id.HasValue).Select(o => o.Id!.Value).ToList();

            // Remove deleted options
            var optionsToRemove = question.Options.Where(o => !requestOptionIds.Contains(o.Id)).ToList();
            _optionRepository.RemoveRange(optionsToRemove);

            // Update existing and add new options
            foreach (var optionDto in request.Options)
            {
                if (optionDto.Id.HasValue)
                {
                    // Update existing option
                    var existingOption = question.Options.FirstOrDefault(o => o.Id == optionDto.Id.Value);
                    if (existingOption != null)
                    {
                        existingOption.Text = optionDto.Text;
                        existingOption.IsCorrect = optionDto.IsCorrect;
                        existingOption.UpdatedAt = DateTime.UtcNow;
                        _optionRepository.Update(existingOption);
                    }
                }
                else
                {
                    // Add new option
                    var newOption = new QuestionOption
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = question.Id,
                        Text = optionDto.Text,
                        IsCorrect = optionDto.IsCorrect,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _optionRepository.AddAsync(newOption);
                }
            }

            var result = await _questionRepository.SaveChangesAsync();
            return result
                ? BaseResponse<string>.Ok(null!, "Question updated successfully.")
                : BaseResponse<string>.Fail("Failed to update question.");
        }

        public async Task<BaseResponse<string>> DeleteQuestionAsync(Guid id)
        {
            var question = await _questionRepository.GetByConditionAsync(
                predicate: q => q.Id == id && q.DeletedAt == null,
                include: q => q.Include(x => x.Options)
            );

            if (question == null)
            {
                return BaseResponse<string>.Fail("Question not found.");
            }

            // Soft delete: mark DeletedAt for the question
            question.DeletedAt = DateTime.UtcNow;
            _questionRepository.Update(question);

            var result = await _questionRepository.SaveChangesAsync();
            return result
                ? BaseResponse<string>.Ok(null!, "Question deleted successfully.")
                : BaseResponse<string>.Fail("Failed to delete question.");
        }

        public async Task<BaseResponse<List<string>>> CreateQuestionsBulkAsync(List<CreateQuestionRequest> requests)
        {
            var ids = new List<string>();
            foreach (var req in requests)
            {
                // Validate LessonPlan existence per request
                var hasLesson = await _lessonRepository.AnyAsync(lp => lp.Id == req.LessonPlanId);
                if (!hasLesson)
                {
                    return BaseResponse<List<string>>.Fail($"LessonPlan not found for request with text '{req.Text}'.");
                }

                var question = new Question
                {
                    Id = Guid.NewGuid(),
                    Text = req.Text,
                    QuestionType = req.QuestionType,
                    DifficultyLevel = req.DifficultyLevel,
                    AwarenessLevel = req.AwarenessLevel,
                    LessonPlanId = req.LessonPlanId,
                    DeletedAt = null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _questionRepository.AddAsync(question);
                ids.Add(question.Id.ToString());

                foreach (var optionDto in req.Options)
                {
                    var option = new QuestionOption
                    {
                        Id = Guid.NewGuid(),
                        QuestionId = question.Id,
                        Text = optionDto.Text,
                        IsCorrect = optionDto.IsCorrect,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _optionRepository.AddAsync(option);
                }
            }

            var saved = await _questionRepository.SaveChangesAsync();
            return saved
                ? BaseResponse<List<string>>.Ok(ids, "Questions created successfully.")
                : BaseResponse<List<string>>.Fail("Failed to create questions.");
        }

        public async Task<byte[]> ExportQuestionsPdfAsync(Guid lessonPlanId, GetQuestionsRequest? filterRequest = null)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            
            var request = filterRequest ?? new GetQuestionsRequest
            {
                PageNumber = 1,
                PageSize = 1000,
                LessonPlanId = lessonPlanId
            };
            request.LessonPlanId = lessonPlanId;

            var result = await GetAllQuestionsAsync(request);
            var questions = result.Success && result.Payload != null ? result.Payload.Items : new List<GetQuestionResponse>();

            var lesson = await _lessonRepository.GetByConditionAsync(
                lp => lp.Id == lessonPlanId,
                asNoTracking: true
            );

            using var stream = new MemoryStream();
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Content()
                        .Column(column =>
                        {
                            column.Spacing(10);

                            // Header
                            column.Item().Text("Ngân hàng câu hỏi").FontSize(16).Bold();
                            if (lesson != null)
                            {
                                column.Item().Text($"Bài giảng: {lesson.Title}").FontSize(13);
                                if (!string.IsNullOrWhiteSpace(lesson.Objective))
                                {
                                    column.Item().Text($"Mục tiêu: {lesson.Objective}").FontSize(11);
                                }
                            }
                            column.Item().Text($"Tổng số câu hỏi: {questions.Count} | Ngày xuất: {DateTime.Now:dd/MM/yyyy}")
                                .FontSize(10);

                            column.Item().PaddingTop(10);

                            // Questions
                            for (int i = 0; i < questions.Count; i++)
                            {
                                var q = questions[i];
                                var questionNumber = i + 1;

                                column.Item().PaddingBottom(5).Column(questionColumn =>
                                {
                                    questionColumn.Item().Text($"{questionNumber}. {q.Text}").Bold();

                                    if (q.QuestionType == "Theory" && q.Options != null && q.Options.Any())
                                    {
                                        foreach (var opt in q.Options)
                                        {
                                            var optionText = $"   - {opt.Text}";
                                            if (opt.IsCorrect)
                                            {
                                                questionColumn.Item().Text(optionText)
                                                    .FontColor(Colors.Red.Medium)
                                                    .Bold();
                                            }
                                            else
                                            {
                                                questionColumn.Item().Text(optionText);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        questionColumn.Item().Text("   Tự luận - Không có đáp án lựa chọn")
                                            .Italic();
                                    }
                                });
                            }
                        });
                });
            });

            document.GeneratePdf(stream);
            return stream.ToArray();
        }
    }
}
