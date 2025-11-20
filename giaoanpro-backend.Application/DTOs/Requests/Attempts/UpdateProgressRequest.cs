using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Requests.Attempts
{
    public class StudentAnswerRequest
    {
        public Guid QuestionId { get; set; }
        public Guid? SelectedOptionId { get; set; }
        public string? TextAnswer { get; set; }
    }

    public class UpdateProgressRequest
    {
        public Guid AttemptId { get; set; }
        public List<StudentAnswerRequest> Answers { get; set; } = new();
    }
}
