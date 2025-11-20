using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Requests.Exams
{
    public class CreateExamRequest
    {
        public Guid MatrixId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Guid> QuestionIds { get; set; } = new List<Guid>();

        // New: allow submitting newly generated question objects alongside existing IDs
        public List<Questions.CreateQuestionRequest>? NewQuestions { get; set; } = new List<Questions.CreateQuestionRequest>();
    }
}
