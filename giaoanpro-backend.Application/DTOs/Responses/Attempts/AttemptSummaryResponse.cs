using System;

namespace giaoanpro_backend.Application.DTOs.Responses.Attempts
{
    public class AttemptSummaryResponse
    {
        public Guid AttemptId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty; // InProgress, Submitted, PendingGrading, Completed
        public double? TotalScore { get; set; }
        public string? Grade { get; set; }
    }
}
