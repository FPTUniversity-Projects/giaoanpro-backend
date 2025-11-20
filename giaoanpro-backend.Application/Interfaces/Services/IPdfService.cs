namespace giaoanpro_backend.Application.Interfaces.Services
{
    public interface IPdfService
    {
        /// <summary>
        /// Generate a PDF from lesson plan and its activities
        /// </summary>
        /// <param name="lessonPlanId">ID of the lesson plan</param>
        /// <returns>PDF content as byte array</returns>
        Task<byte[]> GenerateLessonPlanPdfAsync(Guid lessonPlanId);
    }
}
