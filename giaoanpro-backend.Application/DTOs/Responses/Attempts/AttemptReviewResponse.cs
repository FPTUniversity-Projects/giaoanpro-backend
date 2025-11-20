using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Responses.Attempts
{
    public class OptionReviewDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public bool? IsCorrect { get; set; }
    }

    public class AttemptDetailReviewDto
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty;
        public List<OptionReviewDto> Options { get; set; } = new();
        public string? StudentAnswer { get; set; }
        public bool? IsCorrect { get; set; }
        public decimal? Score { get; set; }
        public string? TeacherFeedback { get; set; }
    }

    public class AttemptReviewResponse : AttemptSummaryResponse
    {
        public List<AttemptDetailReviewDto> Details { get; set; } = new();
    }
}
