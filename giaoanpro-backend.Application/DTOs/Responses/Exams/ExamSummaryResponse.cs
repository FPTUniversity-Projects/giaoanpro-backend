using System;

namespace giaoanpro_backend.Application.DTOs.Responses.Exams
{
    public class ExamSummaryResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public int QuestionCount { get; set; }
        public Guid? ActivityId { get; set; }
    }
}
