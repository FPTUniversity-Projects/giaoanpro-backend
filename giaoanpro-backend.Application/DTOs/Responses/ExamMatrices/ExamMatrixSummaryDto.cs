using System;

namespace giaoanpro_backend.Application.DTOs.Responses.ExamMatrices
{
    public class ExamMatrixSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int Duration { get; set; }
    }
}
