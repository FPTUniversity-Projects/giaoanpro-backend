using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace giaoanpro_backend.Application.DTOs.Responses.LessonPlans
{
    public class LessonPlanResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Objective { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public int ActivityCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
