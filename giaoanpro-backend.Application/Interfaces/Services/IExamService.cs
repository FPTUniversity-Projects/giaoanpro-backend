using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using giaoanpro_backend.Application.DTOs.Requests.Exams;
using giaoanpro_backend.Application.DTOs.Responses.Exams;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Requests.Questions;

namespace giaoanpro_backend.Application.Interfaces.Services
{
    public interface IExamService
    {
        Task<BaseResponse<string>> CreateExamAsync(CreateExamRequest request, Guid teacherId);
        Task<BaseResponse<GetExamDetailResponse>> GetExamByIdAsync(Guid id);
        Task<BaseResponse<List<ExamSummaryResponse>>> GetTeacherExamInventoryAsync(Guid teacherId);
        Task<BaseResponse<GetExamsPagedResponse>> GetTeacherInventoryAsync(GetExamInventoryRequest request, Guid teacherId);
        Task<BaseResponse<string>> UpdateExamAsync(UpdateExamRequest request, Guid teacherId);
        Task<BaseResponse<string>> DeleteExamAsync(Guid id, Guid teacherId);
        Task<BaseResponse<string>> DeleteExamAsync(Guid id); // soft-delete without user check
        Task<BaseResponse<List<CreateQuestionRequest>>> GenerateQuestionsWithAIAsync(GenerateQuestionPromptRequest request);
    }
}
