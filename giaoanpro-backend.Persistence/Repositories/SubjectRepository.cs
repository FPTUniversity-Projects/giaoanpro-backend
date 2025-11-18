using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
	{
		private readonly GiaoanproDBContext _context;

		public SubjectRepository(GiaoanproDBContext context) : base(context)
		{
			_context = context;
		}

		public async Task<bool> ExistsByNameAndGradeAsync(string name, Guid gradeId, Guid? excludeId = null)
		{
			var query = _context.Subjects.Where(s => 
				s.Name.ToLower() == name.ToLower() && s.GradeId == gradeId
			);
			
			if (excludeId.HasValue)
			{
				query = query.Where(s => s.Id != excludeId.Value);
			}

			return await query.AnyAsync();
		}

		public async Task<IEnumerable<Subject>> GetByGradeIdAsync(Guid gradeId)
		{
			return await _context.Subjects
				.Include(s => s.Grade)
				.Include(s => s.Syllabus)
				.Where(s => s.GradeId == gradeId)
				.OrderBy(s => s.Name)
				.ToListAsync();
		}
	}
}
