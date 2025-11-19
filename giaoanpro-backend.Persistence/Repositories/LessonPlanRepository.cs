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
    public class LessonPlanRepository : GenericRepository<LessonPlan>, ILessonPlanRepository
    {
        public LessonPlanRepository(GiaoanproDBContext context) : base(context)
        {
        }

        public async Task<LessonPlan?> GetByIdWithActivitiesAsync(Guid id)
        {
            return await FirstOrDefaultAsync(
                predicate: lp => lp.Id == id,
                include: q => q
                    .Include(lp => lp.Subject)
                    .Include(lp => lp.User)
                    .Include(lp => lp.Activities),
                asNoTracking: true
            );
        }

        public async Task<bool> ExistsByTitleAndUserAsync(string title, Guid userId, Guid? excludeId = null)
        {
            return await AnyAsync(lp =>
                lp.Title.ToLower() == title.ToLower() &&
                lp.UserId == userId &&
                (!excludeId.HasValue || lp.Id != excludeId.Value)
            );
        }
    }
}
