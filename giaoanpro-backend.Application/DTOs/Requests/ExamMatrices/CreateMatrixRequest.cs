using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Requests.ExamMatrices
{
    public class CreateMatrixRequest
    {
        public string Name { get; set; } = string.Empty;
        public Guid SubjectId { get; set; }
        public Guid LessonPlanId { get; set; }
        public List<MatrixDetailRequest> Details { get; set; } = new();
    }
}
