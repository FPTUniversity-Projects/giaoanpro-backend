using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Persistence.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Persistence.Repositories
{
    public class ExamRepository : GenericRepository<Exam>, IExamRepository
    {
        private readonly GiaoanproDBContext _db;

        public ExamRepository(GiaoanproDBContext db) : base(db)
        {
            _db = db;
        }

        public async Task<Exam?> GetExamWithDetailsAsync(Guid id)
        {
            var exam = await _db.Exams
                .Include(e => e.ExamQuestions)
                    .ThenInclude(eq => eq.Question)
                        .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(e => e.Id == id);

            return exam;
        }
    }
}
