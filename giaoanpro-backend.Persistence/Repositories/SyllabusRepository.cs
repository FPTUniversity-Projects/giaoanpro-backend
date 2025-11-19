using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class SyllabusRepository : GenericRepository<Syllabus>, ISyllabusRepository
	{
		private readonly GiaoanproDBContext _context;

		public SyllabusRepository(GiaoanproDBContext context) : base(context)
		{
			_context = context;
		}

		public async Task<Syllabus?> GetBySubjectIdAsync(Guid subjectId)
		{
			return await _context.Syllabuses
				.Include(s => s.Subject)
					.ThenInclude(sub => sub.Grade)
				.FirstOrDefaultAsync(s => s.SubjectId == subjectId);
		}

		public async Task<bool> ExistsBySubjectIdAsync(Guid subjectId, Guid? excludeId = null)
		{
			var query = _context.Syllabuses.Where(s => s.SubjectId == subjectId);
			
			if (excludeId.HasValue)
			{
				query = query.Where(s => s.Id != excludeId.Value);
			}

			return await query.AnyAsync();
		}
	}
}
