using System;
using System.Collections.Generic;
using giaoanpro_backend.Domain.Enums;

namespace giaoanpro_backend.Application.DTOs.Responses.Attempts
{
    public class OptionPaperDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
    }

    public class QuestionPaperDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = string.Empty; // keep as string for FE
        public List<OptionPaperDto> Options { get; set; } = new();
    }
}
