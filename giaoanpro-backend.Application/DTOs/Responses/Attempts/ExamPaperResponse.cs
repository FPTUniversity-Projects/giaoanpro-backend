using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Responses.Attempts
{
    public class ExamPaperResponse
    {
        public Guid AttemptId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public DateTime StartTime { get; set; }
        public List<QuestionPaperDto> Questions { get; set; } = new();
    }
}
