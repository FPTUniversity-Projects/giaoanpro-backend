using giaoanpro_backend.Application.Interfaces.Repositories;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Persistence.Context;
using giaoanpro_backend.Persistence.Repositories.Bases;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Persistence.Repositories
{
	public class ClassMemberRepository : GenericRepository<ClassMember>, IClassMemberRepository
	{
		private readonly GiaoanproDBContext _context;

		public ClassMemberRepository(GiaoanproDBContext context) : base(context)
		{
			_context = context;
		}

		public async Task<(IEnumerable<ClassMember> Items, int TotalCount)> GetPagedByStudentAsync(
			Guid studentId,
			string? className = null,
			Guid? teacherId = null,
			Guid? gradeId = null,
			Guid? semesterId = null,
			int pageNumber = 1,
			int pageSize = 10)
		{
			IQueryable<ClassMember> query = _context.ClassMembers
				.Include(cm => cm.Class)
					.ThenInclude(c => c.Teacher)
				.Include(cm => cm.Class)
					.ThenInclude(c => c.Grade)
				.Include(cm => cm.Class)
					.ThenInclude(c => c.Semester)
				.Where(cm => cm.StudentId == studentId);

			if (!string.IsNullOrEmpty(className))
			{
				query = query.Where(cm => cm.Class.Name.ToLower().Contains(className.ToLower()));
			}

			if (teacherId.HasValue)
			{
				query = query.Where(cm => cm.Class.TeacherId == teacherId.Value);
			}

			if (gradeId.HasValue)
			{
				query = query.Where(cm => cm.Class.GradeId == gradeId.Value);
			}

			if (semesterId.HasValue)
			{
				query = query.Where(cm => cm.Class.SemesterId == semesterId.Value);
			}

			var total = await query.CountAsync();

			var items = await query
				.OrderBy(cm => cm.Class.Name)
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.AsNoTracking()
				.ToListAsync();

			return (items, total);
		}
	}
}
