using System;

namespace giaoanpro_backend.Application.DTOs.Requests.Exams
{
    public class GetExamInventoryRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchText { get; set; }
    }
}
