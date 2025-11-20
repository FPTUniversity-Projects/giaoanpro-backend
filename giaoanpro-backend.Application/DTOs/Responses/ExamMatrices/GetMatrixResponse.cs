using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Responses.ExamMatrices
{
    public class MatrixDetailResponse
    {
        public Guid Id { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string DifficultyLevel { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class GetMatrixResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid SubjectId { get; set; }
        public List<MatrixDetailResponse> Lines { get; set; } = new();
    }
}
