using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Responses.ExamMatrices
{
    public class GetMatrixLineResponse
    {
        public Guid Id { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public int Count { get; set; }
        public int PointsPerQuestion { get; set; }
    }

    public class GetMatrixDetailResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int DurationMinutes { get; set; }
        public Guid SubjectId { get; set; }
        public List<GetMatrixLineResponse> Lines { get; set; } = new();
    }
}
