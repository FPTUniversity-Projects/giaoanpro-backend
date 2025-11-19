using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class ClassRepository : GenericRepository<Class>, IClassRepository
	{
		private readonly GiaoanproDBContext _context;

		public ClassRepository(GiaoanproDBContext context) : base(context)
		{
			_context = context;
		}

		public async Task<bool> ExistsByNameAndSemesterAsync(string name, Guid semesterId, Guid? excludeId = null)
		{
			var query = _context.Classes.Where(c => 
				c.Name.ToLower() == name.ToLower() && c.SemesterId == semesterId
			);
			
			if (excludeId.HasValue)
			{
				query = query.Where(c => c.Id != excludeId.Value);
			}

			return await query.AnyAsync();
		}

		public async Task<IEnumerable<Class>> GetByTeacherIdAsync(Guid teacherId)
		{
			return await _context.Classes
				.Include(c => c.Teacher)
				.Include(c => c.Grade)
				.Include(c => c.Semester)
				.Where(c => c.TeacherId == teacherId)
				.OrderBy(c => c.Name)
				.ToListAsync();
		}

		public async Task<IEnumerable<Class>> GetBySemesterIdAsync(Guid semesterId)
		{
			return await _context.Classes
				.Include(c => c.Teacher)
				.Include(c => c.Grade)
				.Include(c => c.Semester)
				.Where(c => c.SemesterId == semesterId)
				.OrderBy(c => c.Name)
				.ToListAsync();
		}

		public async Task<IEnumerable<Class>> GetByGradeIdAsync(Guid gradeId)
		{
			return await _context.Classes
				.Include(c => c.Teacher)
				.Include(c => c.Grade)
				.Include(c => c.Semester)
				.Where(c => c.GradeId == gradeId)
				.OrderBy(c => c.Name)
				.ToListAsync();
		}

		public async Task<Class?> GetWithMembersAsync(Guid classId)
		{
			return await _context.Classes
				.Include(c => c.Teacher)
				.Include(c => c.Grade)
				.Include(c => c.Semester)
				.Include(c => c.Members)
					.ThenInclude(m => m.Student)
				.FirstOrDefaultAsync(c => c.Id == classId);
		}
	}
}
