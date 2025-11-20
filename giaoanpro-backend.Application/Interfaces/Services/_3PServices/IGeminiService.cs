using giaoanpro_backend.Application.DTOs.Requests.Questions;

namespace giaoanpro_backend.Application.Interfaces.Services._3PServices
{
    public interface IGeminiService
    {
        Task<string> GenerateQuestionsJsonAsync(Guid lessonPlanId, string context, GenerateQuestionSpec spec, int count, CancellationToken ct = default);
        Task<string> GenerateContentAsync(string prompt, CancellationToken ct = default);
    }
}
