using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;

namespace giaoanpro_backend.Application.Services
{
    public class LessonPlanPdfManager : ILessonPlanPdfManager
    {
        private readonly IPdfService _pdfService;
        private readonly IS3Service _s3Service;

        public LessonPlanPdfManager(IPdfService pdfService, IS3Service s3Service)
        {
            _pdfService = pdfService;
            _s3Service = s3Service;
        }

        public async Task<string> GenerateAndUploadPdfAsync(Guid lessonPlanId)
        {
            // Generate PDF
            var pdfBytes = await _pdfService.GenerateLessonPlanPdfAsync(lessonPlanId);

            // Upload to S3
            var fileName = $"lesson-plan-{lessonPlanId}.pdf";
            var fileUrl = await _s3Service.UploadFileAsync(fileName, pdfBytes, "application/pdf");

            return fileUrl;
        }

        public async Task DeletePdfAsync(string fileUrl)
        {
            if (!string.IsNullOrWhiteSpace(fileUrl))
            {
                await _s3Service.DeleteFileAsync(fileUrl);
            }
        }
    }
}
