using giaoanpro_backend.Application.DTOs.Requests.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace giaoanpro_backend.Application.DTOs.Requests.Activities
{
    public class GetActivitiesQuery : PagingAndSortingQuery
    {
        public Guid? LessonPlanId { get; set; }
        public Guid? ParentId { get; set; }
        //public int PageNumber { get; set; } = 1;
        //public int PageSize { get; set; } = 10;
    }
}
