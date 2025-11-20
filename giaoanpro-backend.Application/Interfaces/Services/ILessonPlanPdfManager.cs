namespace giaoanpro_backend.Application.Interfaces.Services
{
    public interface ILessonPlanPdfManager
    {
        /// <summary>
        /// Generate and upload PDF for a lesson plan
        /// </summary>
        /// <param name="lessonPlanId">ID of the lesson plan</param>
        /// <returns>URL of the uploaded PDF</returns>
        Task<string> GenerateAndUploadPdfAsync(Guid lessonPlanId);

        /// <summary>
        /// Delete PDF file from storage
        /// </summary>
        /// <param name="fileUrl">URL of the file to delete</param>
        Task DeletePdfAsync(string fileUrl);
    }
}
