using FluentValidation;
using giaoanpro_backend.Application.DTOs.Requests.Questions;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Questions;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Domain.Entities;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace giaoanpro_backend.Application.Services
{
	public class QuestionService : IQuestionService
	{
		private readonly IGenericRepository<Question> _questionRepository;
		private readonly IGenericRepository<QuestionOption> _optionRepository;
		private readonly IMapper _mapper;

		public QuestionService(
			IGenericRepository<Question> questionRepository,
			IGenericRepository<QuestionOption> optionRepository,
			IMapper mapper)
		{
			_questionRepository = questionRepository;
			_optionRepository = optionRepository;
			_mapper = mapper;
		}

		public async Task<BaseResponse<GetQuestionsPagedResponse>> GetAllQuestionsAsync(GetQuestionsRequest request)
		{
			// Build filter expression (always exclude soft-deleted)
			Expression<Func<Question, bool>>? filter = null;

			if (request.QuestionType.HasValue || request.DifficultyLevel.HasValue || request.AwarenessLevel.HasValue || !string.IsNullOrWhiteSpace(request.SearchText))
			{
				filter = q =>
					q.DeletedAt == null &&
					(!request.QuestionType.HasValue || q.QuestionType == request.QuestionType.Value) &&
					(!request.DifficultyLevel.HasValue || q.DifficultyLevel == request.DifficultyLevel.Value) &&
					(!request.AwarenessLevel.HasValue || q.AwarenessLevel == request.AwarenessLevel.Value) &&
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
				include: q => q.Include(x => x.Options),
				asNoTracking: true
			);

			if (question == null)
			{
				return BaseResponse<GetQuestionResponse>.Fail("Question not found.", ResponseErrorType.NotFound);
			}

			var response = _mapper.Map<GetQuestionResponse>(question);
			return BaseResponse<GetQuestionResponse>.Ok(response, "Question retrieved successfully.");
		}

		public async Task<BaseResponse<string>> CreateQuestionAsync(CreateQuestionRequest request)
		{


			// Create question
			var question = new Question
			{
				Id = Guid.NewGuid(),
				Text = request.Text,
				QuestionType = request.QuestionType,
				DifficultyLevel = request.DifficultyLevel,
				AwarenessLevel = request.AwarenessLevel,
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
				: BaseResponse<string>.Fail("Failed to create question.", ResponseErrorType.InternalError);
		}

		public async Task<BaseResponse<string>> UpdateQuestionAsync(Guid id, UpdateQuestionRequest request)
		{


			var question = await _questionRepository.GetByConditionAsync(
				predicate: q => q.Id == id && q.DeletedAt == null,
				include: q => q.Include(x => x.Options)
			);

			if (question == null)
			{
				return BaseResponse<string>.Fail("Question not found.", ResponseErrorType.NotFound);
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
				: BaseResponse<string>.Fail("Failed to update question.", ResponseErrorType.InternalError);
		}

		public async Task<BaseResponse<string>> DeleteQuestionAsync(Guid id)
		{
			var question = await _questionRepository.GetByConditionAsync(
				predicate: q => q.Id == id && q.DeletedAt == null,
				include: q => q.Include(x => x.Options)
			);

			if (question == null)
			{
				return BaseResponse<string>.Fail("Question not found.", ResponseErrorType.NotFound);
			}

			// Soft delete: mark DeletedAt for the question
			question.DeletedAt = DateTime.UtcNow;
			_questionRepository.Update(question);

			var result = await _questionRepository.SaveChangesAsync();
			return result
				? BaseResponse<string>.Ok(null!, "Question deleted successfully.")
				: BaseResponse<string>.Fail("Failed to delete question.", ResponseErrorType.InternalError);
		}

		public async Task<BaseResponse<List<string>>> CreateQuestionsBulkAsync(List<CreateQuestionRequest> requests)
		{
			var ids = new List<string>();
			foreach (var req in requests)
			{


				var question = new Question
				{
					Id = Guid.NewGuid(),
					Text = req.Text,
					QuestionType = req.QuestionType,
					DifficultyLevel = req.DifficultyLevel,
					AwarenessLevel = req.AwarenessLevel,
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
				: BaseResponse<List<string>>.Fail("Failed to create questions.", ResponseErrorType.InternalError);
		}
	}
}
