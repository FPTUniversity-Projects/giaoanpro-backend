using System;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.DTOs.Responses.ExamMatrices
{
    public class GetMatrixPagedResponse
    {
        public List<ExamMatrixSummaryDto> Items { get; set; } = new();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}
