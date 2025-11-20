using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Requests.Attempts
{
    public class SubmitAttemptRequest
    {
        public Guid AttemptId { get; set; }
        public List<StudentAnswerRequest> Answers { get; set; } = new();
    }
}
