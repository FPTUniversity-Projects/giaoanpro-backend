using giaoanpro_backend.Application.DTOs.Requests.Bases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace giaoanpro_backend.Application.DTOs.Requests.LessonPlans
{
    public class GetLessonPlansQuery : PagingAndSortingQuery
    {
        public Guid? ClassId { get; set; }
        
        public string? Title { get; set; }
        public Guid? SubjectId { get; set; }
        public Guid? UserId { get; set; }
    }
}
