using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Requests.Attempts
{
    public class QuestionGrade
    {
        public Guid QuestionId { get; set; }
        public decimal Score { get; set; }
        public string? Feedback { get; set; }
    }

    public class GradeAttemptRequest
    {
        public Guid AttemptId { get; set; }
        public List<QuestionGrade> Grades { get; set; } = new();
    }
}
