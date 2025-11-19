using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
    public interface IActivityRepository : IGenericRepository<Activity>
    {
        Task<Activity?> GetByIdWithChildrenAsync(Guid id);
        Task<IEnumerable<Activity>> GetByLessonPlanIdAsync(Guid lessonPlanId);
    }
}
