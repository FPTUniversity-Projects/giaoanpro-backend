using System;

namespace giaoanpro_backend.Application.DTOs.Requests.ExamMatrices
{
    public class GetMatrixPagedRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchText { get; set; }
        public Guid? SubjectId { get; set; }
    }
}
