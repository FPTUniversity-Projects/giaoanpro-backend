using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;

namespace giaoanpro_backend.Persistence.Repositories
{
    public class ExamMatrixRepository : GenericRepository<ExamMatrix>, IExamMatrixRepository
    {
        public ExamMatrixRepository(GiaoanproDBContext context) : base(context)
        {
        }
    }
}
