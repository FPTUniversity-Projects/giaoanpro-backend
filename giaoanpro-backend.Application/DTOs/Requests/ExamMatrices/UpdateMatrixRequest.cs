using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Requests.ExamMatrices
{
    public class UpdateMatrixRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int TotalQuestions { get; set; }
        public int DurationMinutes { get; set; }
        public List<MatrixDetailRequest> Details { get; set; } = new();
    }
}
