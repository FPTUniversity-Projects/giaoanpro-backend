using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Requests.Exams
{
    public class CreateExamRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public List<Guid> QuestionIds { get; set; } = new List<Guid>();
    }
}
