using System;

namespace giaoanpro_backend.Application.DTOs.Responses.Attempts
{
    public class AttemptResultResponse
    {
        public Guid AttemptId { get; set; }
        public decimal AutoScore { get; set; }
        public decimal? ManualScore { get; set; }
        public decimal TotalScore { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
