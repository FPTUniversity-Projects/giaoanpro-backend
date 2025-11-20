using giaoanpro_backend.Application.DTOs.Requests.Questions;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Questions;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface IQuestionService
	{
		Task<BaseResponse<GetQuestionsPagedResponse>> GetAllQuestionsAsync(GetQuestionsRequest request, Guid userId);
		Task<BaseResponse<GetQuestionResponse>> GetQuestionByIdAsync(Guid id);
		Task<BaseResponse<string>> CreateQuestionAsync(CreateQuestionRequest request, Guid userId);
		Task<BaseResponse<string>> UpdateQuestionAsync(Guid id, UpdateQuestionRequest request);
		Task<BaseResponse<string>> DeleteQuestionAsync(Guid id);
		Task<BaseResponse<List<string>>> CreateQuestionsBulkAsync(List<CreateQuestionRequest> requests, Guid userId);
		Task<BaseResponse<List<GetQuestionResponse>>> GenerateQuestionsAiAsync(GenerateQuestionsRequest request, Guid userId);
		Task<byte[]> ExportQuestionsPdfAsync(Guid lessonPlanId, GetQuestionsRequest? filterRequest, Guid userId);
	}
}
