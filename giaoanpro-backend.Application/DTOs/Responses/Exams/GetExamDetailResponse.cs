using System;
using System.Collections.Generic;
using giaoanpro_backend.Application.DTOs.Responses.Questions;

namespace giaoanpro_backend.Application.DTOs.Responses.Exams
{
    public class GetExamDetailResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ActivityId { get; set; }
        public List<GetQuestionResponse> Questions { get; set; } = new List<GetQuestionResponse>();
    }
}
