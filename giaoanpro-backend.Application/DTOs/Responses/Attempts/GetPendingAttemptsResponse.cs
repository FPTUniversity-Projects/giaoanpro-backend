using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Responses.Attempts
{
    public class PendingAttemptDto
    {
        public Guid AttemptId { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public Guid ExamId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    public class GetPendingAttemptsResponse
    {
        public List<PendingAttemptDto> Items { get; set; } = new();
    }
}
