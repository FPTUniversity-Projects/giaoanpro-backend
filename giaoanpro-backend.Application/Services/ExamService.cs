using giaoanpro_backend.Application.DTOs.Requests.Exams;
using giaoanpro_backend.Application.DTOs.Requests.Questions;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Exams;
using giaoanpro_backend.Application.DTOs.Responses.Questions;
using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace giaoanpro_backend.Application.Services
{
	public class ExamService : IExamService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IExamRepository _examRepository;
		private readonly IQuestionRepository _question_repository;
		private readonly IGenericRepository<Question> _questionGeneric;
		private readonly IMapper _mapper;
		private readonly IGeminiService _geminiService;

		public ExamService(IUnitOfWork unitOfWork, IExamRepository examRepository, IQuestionRepository questionRepository, IGenericRepository<Question> questionGeneric, IMapper mapper, IGeminiService geminiService)
		{
			_unitOfWork = unitOfWork;
			_examRepository = examRepository;
			_question_repository = questionRepository;
			_questionGeneric = questionGeneric;
			_mapper = mapper;
			_geminiService = geminiService;
		}

		public async Task<BaseResponse<string>> CreateExamAsync(CreateExamRequest request, Guid teacherId)
		{
			if (request == null)
				return BaseResponse<string>.Fail("Request cannot be null.", ResponseErrorType.BadRequest);

			// Validate MatrixId
			var matrix = await _unitOfWork.ExamMatrices.GetByConditionAsync(
				m => m.Id == request.MatrixId,
				include: q => q.Include(m => m.Lines),
				asNoTracking: false
			);

			if (matrix == null)
				return BaseResponse<string>.Fail("Matrix not found.", ResponseErrorType.NotFound);

			// Fetch selected existing questions by IDs
			var questionEntities = new List<Question>();
			if (request.QuestionIds != null && request.QuestionIds.Any())
			{
				var (items, total) = await _question_repository.GetPagedWithOptionsAsync(
					filter: q => request.QuestionIds.Contains(q.Id),
					pageNumber: 1,
					pageSize: request.QuestionIds.Count,
					asNoTracking: false
				);
				questionEntities = items.ToList();
			}

			// Basic check: ensure all provided IDs exist
			if (questionEntities.Count != (request.QuestionIds?.Count ?? 0))
				return BaseResponse<string>.Fail("Some selected questions were not found.", ResponseErrorType.BadRequest);

			// Build combined selected items (existing entities + new question DTOs) for matrix validation
			// Represent each as tuple of (QuestionTypeName, DifficultyLevelName)
			var combinedKeys = new List<(string qType, string diff)>();

			// Existing entities
			combinedKeys.AddRange(questionEntities.Select(q => (q.QuestionType.ToString(), q.DifficultyLevel.ToString())));

			// New questions from request (may be null or empty)
			if (request.NewQuestions != null && request.NewQuestions.Any())
			{
				foreach (var nq in request.NewQuestions)
				{
					// Use provided enum values directly
					combinedKeys.Add((nq.QuestionType.ToString(), nq.DifficultyLevel.ToString()));
				}
			}

			// Build selectedGroups from combined keys
			var selectedGroups = combinedKeys
				.GroupBy(k => (k.qType, k.diff))
				.ToDictionary(g => g.Key, g => g.Count());

			// Build expected groups from matrix lines (already strings)
			var expectedGroups = matrix.Lines
				.GroupBy(l => (l.QuestionType, l.DifficultyLevel))
				.ToDictionary(g => g.Key, g => g.Sum(l => l.Count));

			// Compare expected vs selected exactly
			bool matches = expectedGroups.Count == selectedGroups.Count && expectedGroups.All(kv => selectedGroups.TryGetValue(kv.Key, out var cnt) && cnt == kv.Value);

			if (!matches)
			{
				return BaseResponse<string>.Fail("Selected questions (existing + new) do not match the Matrix structure.", ResponseErrorType.BadRequest);
			}

			var now = DateTime.UtcNow;

			var exam = new Exam
			{
				Id = Guid.NewGuid(),
				CreatorId = teacherId,
				Title = request.Title,
				Description = request.Description,
				MatrixId = matrix.Id,
				DurationMinutes = matrix.TotalQuestions > 0 ? matrix.TotalQuestions : 0,
				ActivityId = null,
				CreatedAt = now,
				UpdatedAt = now
			};

			// Add exam to repository (tracked)
			await _examRepository.AddAsync(exam);

			// Sequence number starts at 1
			var seq = 1;

			// 1) Link existing questions
			foreach (var q in questionEntities)
			{
				var eq = new ExamQuestion
				{
					ExamId = exam.Id,
					QuestionId = q.Id,
					SequenceNumber = seq++,
					CreatedAt = now,
					UpdatedAt = now
				};
				exam.ExamQuestions.Add(eq);
			}

			// 2) Handle NewQuestions: create Question entities (with options) and link them
			if (request.NewQuestions != null && request.NewQuestions.Any())
			{
				foreach (var nq in request.NewQuestions)
				{
					var newQ = new Question
					{
						Id = Guid.NewGuid(),
						Text = nq.Text,
						QuestionType = nq.QuestionType,
						DifficultyLevel = nq.DifficultyLevel,
						AwarenessLevel = nq.AwarenessLevel,
						LessonPlanId = nq.LessonPlanId,
						CreatedAt = now,
						UpdatedAt = now,
						DeletedAt = null
					};

					// Map options (if any) into QuestionOption entities
					if (nq.Options != null && nq.Options.Any())
					{
						foreach (var opt in nq.Options)
						{
							var qo = new QuestionOption
							{
								Id = Guid.NewGuid(),
								QuestionId = newQ.Id,
								Text = opt.Text,
								IsCorrect = opt.IsCorrect,
								CreatedAt = now,
								UpdatedAt = now
							};
							newQ.Options.Add(qo);
						}
					}

					// Add new question to repository
					await _questionGeneric.AddAsync(newQ);

					// Link to exam
					var eqNew = new ExamQuestion
					{
						ExamId = exam.Id,
						QuestionId = newQ.Id,
						SequenceNumber = seq++,
						CreatedAt = now,
						UpdatedAt = now
					};
					exam.ExamQuestions.Add(eqNew);
				}
			}

			// Save all changes once
			var saved = await _unitOfWork.SaveChangesAsync();
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

		public async Task<BaseResponse<string>> UpdateExamAsync(Guid id, UpdateExamRequest request, Guid teacherId)
		{
			if (request == null)
				return BaseResponse<string>.Fail("Request cannot be null.", ResponseErrorType.BadRequest);

			var exam = await _examRepository.GetByConditionAsync(
				predicate: e => e.Id == id,
				include: q => q.Include(e => e.ExamQuestions).ThenInclude(eq => eq.Question).Include(e => e.Matrix).ThenInclude(m => m.Lines)
			);

			if (exam == null)
				return BaseResponse<string>.Fail("Exam not found.", ResponseErrorType.NotFound);

			if (exam.CreatorId != teacherId)
				return BaseResponse<string>.Fail("You do not have permission to update this exam.", ResponseErrorType.Forbidden);

			// Do not allow MatrixId change via update (request doesn't include it). Ensure matrix exists on exam.
			if (exam.Matrix == null)
				return BaseResponse<string>.Fail("Associated matrix not found.", ResponseErrorType.InternalError);

			// Fetch new questions
			var newQuestions = new List<Question>();
			if (request.QuestionIds != null && request.QuestionIds.Any())
			{
				var (items, total) = await _question_repository.GetPagedWithOptionsAsync(
					filter: q => request.QuestionIds.Contains(q.Id),
					pageNumber: 1,
					pageSize: request.QuestionIds.Count,
					asNoTracking: false
				);
				newQuestions = items.ToList();
			}

			if (newQuestions.Count != request.QuestionIds.Count)
				return BaseResponse<string>.Fail("Some selected questions were not found.", ResponseErrorType.BadRequest);

			// For each matrix detail, ensure newQuestions contains exactly required number matching QuestionType & DifficultyLevel
			foreach (var line in exam.Matrix.Lines)
			{
				var required = line.Count;
				var found = newQuestions.Count(q => q.QuestionType.ToString() == line.QuestionType && q.DifficultyLevel.ToString() == line.DifficultyLevel);
				if (found != required)
				{
					return BaseResponse<string>.Fail($"Update failed: Matrix requirement for {line.DifficultyLevel} {line.QuestionType} not met.", ResponseErrorType.BadRequest);
				}
			}

			// All checks passed - perform update
			exam.Title = request.Title;
			exam.Description = request.Description;
			exam.UpdatedAt = DateTime.UtcNow;

			// Existing exam questions handling: keep matching ones, soft-delete removed ones, add new ones — avoid PK collisions
			var now = DateTime.UtcNow;
			var newQuestionIdSet = new HashSet<Guid>(request.QuestionIds ?? Enumerable.Empty<Guid>());

			// Map existing by QuestionId for quick lookup
			var existingMap = (exam.ExamQuestions ?? Enumerable.Empty<ExamQuestion>()).ToDictionary(eq => eq.QuestionId, eq => eq);

			// Soft-delete/exclude existing questions that are not in new set
			foreach (var existing in existingMap.Values)
			{
				if (!newQuestionIdSet.Contains(existing.QuestionId))
				{
					if (existing.DeletedAt == null)
					{
						existing.DeletedAt = now;
						existing.UpdatedAt = now;
					}
				}
				else
				{
					// ensure it's active
					if (existing.DeletedAt != null)
					{
						existing.DeletedAt = null;
						existing.UpdatedAt = now;
					}
				}
			}

			// Add truly new mappings (those in request but not in existingMap)
			int seq = 1;
			foreach (var qid in request.QuestionIds ?? Enumerable.Empty<Guid>())
			{
				if (existingMap.TryGetValue(qid, out var existingEq))
				{
					// update sequence
					existingEq.SequenceNumber = seq++;
					existingEq.UpdatedAt = now;
				}
				else
				{
					var eq = new ExamQuestion
					{
						ExamId = exam.Id,
						QuestionId = qid,
						SequenceNumber = seq++,
						CreatedAt = now,
						UpdatedAt = now,
						DeletedAt = null
					};
					exam.ExamQuestions.Add(eq);
				}
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

		public async Task<BaseResponse<string>> DeleteExamAsync(Guid id)
		{
			var exam = await _examRepository.GetByConditionAsync(
				predicate: e => e.Id == id
			);

			if (exam == null)
				return BaseResponse<string>.Fail("Exam not found.", ResponseErrorType.NotFound);

			exam.DeletedAt = DateTime.UtcNow;
			_examRepository.Update(exam);

			var saved = await _examRepository.SaveChangesAsync();
			if (!saved)
				return BaseResponse<string>.Fail("Failed to delete exam.", ResponseErrorType.InternalError);

			return BaseResponse<string>.Ok(null!, "Exam deleted successfully.");
		}

		public async Task<BaseResponse<List<CreateQuestionRequest>>> GenerateQuestionsWithAIAsync(GenerateQuestionPromptRequest request)
		{
			if (request == null)
				return BaseResponse<List<CreateQuestionRequest>>.Fail("Request cannot be null.", ResponseErrorType.BadRequest);

			// 1. Fetch Context via Matrix -> LessonPlan (read-only)
			var matrix = await _unitOfWork.ExamMatrices.GetByConditionAsync(
				m => m.Id == request.MatrixId,
				include: x => x.Include(m => m.LessonPlan).ThenInclude(lp => lp.Subject),
				asNoTracking: true
			);

			if (matrix == null)
				return BaseResponse<List<CreateQuestionRequest>>.Fail("Matrix not found.", ResponseErrorType.NotFound);

			var lessonPlan = matrix.LessonPlan;
			if (lessonPlan == null)
				return BaseResponse<List<CreateQuestionRequest>>.Fail("Associated Lesson Plan not found.", ResponseErrorType.NotFound);

			// 2. Subject Validation (read-only)
			bool isLiterature = lessonPlan.Subject.Name.Contains("Ngữ văn", StringComparison.OrdinalIgnoreCase) ||
								lessonPlan.Subject.Name.Contains("Literature", StringComparison.OrdinalIgnoreCase);

			if (!isLiterature)
				return BaseResponse<List<CreateQuestionRequest>>.Fail("Tính năng AI chỉ hỗ trợ môn Ngữ văn.", ResponseErrorType.BadRequest);

			// 3. Build prompt
			string typeSpecificInstruction;
			string jsonExample;
			var type = request.QuestionType?.ToLower() ?? string.Empty;

			if (type.Contains("trắc nghiệm") || type.Contains("theory") || type.Contains("multiple") || type.Contains("choice"))
			{
				typeSpecificInstruction = @"
        - Bắt buộc phải có 4 phương án (A, B, C, D).
        - Chỉ 1 phương án có isCorrect = true.
        ";

				jsonExample = "\"options\": [\n  { \"text\": \"Đáp án A\", \"isCorrect\": false },\n  { \"text\": \"Đáp án B\", \"isCorrect\": true },\n  { \"text\": \"Đáp án C\", \"isCorrect\": false },\n  { \"text\": \"Đáp án D\", \"isCorrect\": false }\n]";
			}
			else
			{
				typeSpecificInstruction = @"
        - KHÔNG tạo phương án lựa chọn (A, B, C, D).
        - Để mảng 'options' là rỗng [].
        - Nội dung câu hỏi phải rõ ràng, yêu cầu học sinh tự viết câu trả lời.
        ";

				jsonExample = "\"options\": []";
			}

			string contextData = $"Bài học: {lessonPlan.Title}. Mục tiêu: {lessonPlan.Objective}. Nội dung: {lessonPlan.Note}";
			var awarenessName = request.AwarenessLevel.ToString();

			var prompt = @$"
        VAI TRÒ: Bạn là chuyên gia giáo dục Việt Nam, chuyên soạn câu hỏi môn Ngữ văn.

        NHIỆM VỤ: Dựa vào nội dung sau để tạo {request.Count} câu hỏi:
        ---
        {contextData}
        ---

        YÊU CẦU CHUNG:
        1. Chủ đề: {request.Topic}
        2. Độ khó: {request.DifficultyLevel}
        3. Dạng câu hỏi: {request.QuestionType}
        4. Ngôn ngữ: Tiếng Việt chuẩn.
        5. AwarenessLevel phải là một trong các giá trị chính xác: Remember, Understand, Apply, AdvancedApply (sử dụng đúng chuỗi này).
        
        YÊU CẦU RIÊNG CHO DẠNG CÂU HỎI NÀY:
        {typeSpecificInstruction}

        ĐỊNH DẠNG JSON (Bắt buộc) - SỬ DỤNG TÊN ENUM TIẾNG ANH CHÍNH XÁC:
        {{
          ""questions"": [
            {{
              ""text"": ""Nội dung câu hỏi?"",
              ""questionType"": ""{request.QuestionType}"",
              ""difficultyLevel"": ""{request.DifficultyLevel}"",
              ""awarenessLevel"": ""{awarenessName}"",
              {jsonExample}
            }}
          ]
        }}";

			try
			{
				string aiResponse = await _geminiService.GenerateContentAsync(prompt);
				if (string.IsNullOrWhiteSpace(aiResponse))
					return BaseResponse<List<CreateQuestionRequest>>.Fail("AI returned empty response.", ResponseErrorType.InternalError);

				// Cleanup markdown fences
				aiResponse = aiResponse.Replace("```json", "").Replace("```", "").Trim();

				var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

				AiQuestionWrapperRaw? wrapperRaw = null;
				try
				{
					wrapperRaw = JsonSerializer.Deserialize<AiQuestionWrapperRaw>(aiResponse, options);
				}
				catch (JsonException)
				{
					// Try to extract JSON substring if AI added extra text
					var start = aiResponse.IndexOf('{');
					var end = aiResponse.LastIndexOf('}');
					if (start >= 0 && end > start)
					{
						var sub = aiResponse.Substring(start, end - start + 1);
						try
						{
							wrapperRaw = JsonSerializer.Deserialize<AiQuestionWrapperRaw>(sub, options);
						}
						catch
						{
							return BaseResponse<List<CreateQuestionRequest>>.Fail("Failed to parse AI JSON response.", ResponseErrorType.InternalError);
						}
					}
					else
					{
						return BaseResponse<List<CreateQuestionRequest>>.Fail("Failed to parse AI JSON response.", ResponseErrorType.InternalError);
					}
				}

				if (wrapperRaw?.Questions == null || !wrapperRaw.Questions.Any())
					return BaseResponse<List<CreateQuestionRequest>>.Fail("AI returned valid JSON but no questions found.", ResponseErrorType.InternalError);

				var mapped = new List<CreateQuestionRequest>();
				foreach (var rq in wrapperRaw.Questions)
				{
					var cri = new CreateQuestionRequest
					{
						Text = rq.Text ?? string.Empty,
						LessonPlanId = lessonPlan.Id,
						Options = (rq.Options ?? new List<AiOptionRaw>()).Select(o => new CreateQuestionOptionDto { Text = o.Text ?? string.Empty, IsCorrect = o.IsCorrect }).ToList()
					};

					// QuestionType parsing with graceful fallback
					if (!Enum.TryParse<QuestionType>(rq.QuestionType, true, out var parsedQType))
					{
						var qt = (rq.QuestionType ?? string.Empty).ToLower();
						if (qt.Contains("trắc") || qt.Contains("multiple") || qt.Contains("choice") || qt.Contains("mcq") || qt.Contains("exercise"))
							parsedQType = QuestionType.Exercise;
						else
							parsedQType = QuestionType.Theory;
					}
					cri.QuestionType = parsedQType;

					// DifficultyLevel parsing
					if (!Enum.TryParse<DifficultyLevel>(rq.DifficultyLevel, true, out var parsedDiff))
					{
						parsedDiff = DifficultyLevel.Medium;
					}
					cri.DifficultyLevel = parsedDiff;

					// AwarenessLevel parsing
					if (!Enum.TryParse<AwarenessLevel>(rq.AwarenessLevel, true, out var parsedAw))
					{
						parsedAw = AwarenessLevel.Remember;
					}
					cri.AwarenessLevel = parsedAw;

					// Enforce options rules:
					// - If parsed type is Theory -> ensure at least 3 options (prefer 4). Add placeholder options if AI returned fewer.
					// - If parsed type is Exercise -> clear options (no options expected).
					if (cri.QuestionType == QuestionType.Theory)
					{
						cri.Options ??= new List<CreateQuestionOptionDto>();
						// Ensure at least 3 options (prefer 4)
						int minOptions = 3;
						int preferOptions = 4;
						int existingCount = cri.Options.Count;
						if (existingCount < minOptions)
						{
							// check if any option is marked correct
							bool hasCorrect = cri.Options.Any(o => o.IsCorrect);
							// Add placeholders
							for (int p = 0; p < (preferOptions - existingCount); p++)
							{
								var opt = new CreateQuestionOptionDto
								{
									Text = $"Đáp án bổ sung {p + 1}",
									IsCorrect = !hasCorrect && p == 0 // mark first placeholder correct if no correct exists
								};
								cri.Options.Add(opt);
							}
							// If still no correct (edge case), mark first as correct
							if (!cri.Options.Any(o => o.IsCorrect) && cri.Options.Count > 0)
								cri.Options[0].IsCorrect = true;
						}
					}
					else // Exercise
					{
						// Remove any options for exercise type
						cri.Options = new List<CreateQuestionOptionDto>();
					}

					mapped.Add(cri);
				}

				// Important: this method is stateless and does NOT persist anything to the database.
				return BaseResponse<List<CreateQuestionRequest>>.Ok(mapped, "Questions generated successfully.");
			}
			catch (Exception ex)
			{
				return BaseResponse<List<CreateQuestionRequest>>.Fail($"AI Error: {ex.Message}", ResponseErrorType.InternalError);
			}
		}
	}
}
