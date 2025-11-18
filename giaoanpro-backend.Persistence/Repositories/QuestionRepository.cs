using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace giaoanpro_backend.Persistence.Repositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly GiaoanproDBContext _db;

        public QuestionRepository(GiaoanproDBContext db)
        {
            _db = db;
        }

        public async Task<(List<Question> Items, int TotalCount)> GetPagedWithOptionsAsync(
            Expression<Func<Question, bool>>? filter,
            int pageNumber,
            int pageSize,
            bool asNoTracking = true)
        {
            var query = _db.Questions.AsQueryable();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            // Include options and lesson plan and order by latest created
            query = query
                .Include(q => q.Options)
                .Include(q => q.LessonPlan)
                .OrderByDescending(q => q.CreatedAt);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            var total = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Question?> GetByIdWithOptionsAsync(Guid id, bool asNoTracking = true)
        {
            var query = _db.Questions
                .Include(q => q.Options)
                .Include(q => q.LessonPlan)
                .Where(q => q.Id == id);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync();
        }
    }
}
