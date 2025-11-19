using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace giaoanpro_backend.Persistence.Repositories
{
    public class ActivityRepository : GenericRepository<Activity>, IActivityRepository
    {
        public ActivityRepository(GiaoanproDBContext context) : base(context)
        {
        }

        public async Task<Activity?> GetByIdWithChildrenAsync(Guid id)
        {
            return await FirstOrDefaultAsync(
                predicate: a => a.Id == id,
                include: q => q
                    .Include(a => a.LessonPlan)
                    .Include(a => a.Children),
                asNoTracking: true
            );
        }

        public async Task<IEnumerable<Activity>> GetByLessonPlanIdAsync(Guid lessonPlanId)
        {
            return await GetAllAsync(
                filter: a => a.LessonPlanId == lessonPlanId,
                include: q => q.Include(a => a.Children),
                asNoTracking: true
            );
        }
    }
}
