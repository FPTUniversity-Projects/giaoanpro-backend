using giaoanpro_backend.Application.DTOs.Requests.Attempts;
using giaoanpro_backend.Application.DTOs.Responses.Attempts;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.Interfaces.Services
{
    public interface IAttemptService
    {
        Task<BaseResponse<ExamPaperResponse>> StartAttemptAsync(StartAttemptRequest request, Guid userId);
        Task<BaseResponse<string>> SaveProgressAsync(UpdateProgressRequest request, Guid userId);
        Task<BaseResponse<AttemptResultResponse>> SubmitAttemptAsync(SubmitAttemptRequest request, Guid userId);
        Task<BaseResponse<GetPendingAttemptsResponse>> GetPendingAttemptsAsync(Guid teacherId);
        Task<BaseResponse<string>> GradeAttemptAsync(GradeAttemptRequest request, Guid teacherId);

        // New retrieval methods
        Task<BaseResponse<List<AttemptSummaryResponse>>> GetMyAttemptsAsync(Guid studentId);
        Task<BaseResponse<List<AttemptSummaryResponse>>> GetAttemptsByExamAsync(Guid examId, Guid teacherId);
        Task<BaseResponse<AttemptReviewResponse>> GetAttemptDetailAsync(Guid attemptId, Guid currentUserId);
    }
}
