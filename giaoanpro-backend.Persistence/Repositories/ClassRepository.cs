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

		public async Task<(IEnumerable<ClassMember> Items, int TotalCount)> GetMembersPagedAsync(
			Guid classId,
			string? search = null,
			string? role = null,
			string? sortBy = null,
			bool descending = false,
			int pageNumber = 1,
			int pageSize = 10)
		{
			if (classId == Guid.Empty) return (Enumerable.Empty<ClassMember>(), 0);

			IQueryable<ClassMember> query = _context.ClassMembers
				.Include(cm => cm.Student)
				.Where(cm => cm.ClassId == classId);

			if (!string.IsNullOrWhiteSpace(search))
			{
				var term = search.Trim().ToLower();
				query = query.Where(cm => cm.Student.FullName.ToLower().Contains(term) || cm.Student.Email.ToLower().Contains(term));
			}

			if (!string.IsNullOrWhiteSpace(role))
			{
				var roleTerm = role.Trim().ToLower();
				query = query.Where(cm => cm.Student.Role.ToString().ToLower() == roleTerm);
			}

			// Sorting
			switch (sortBy?.Trim().ToLower())
			{
				case "Email":
					query = descending ? query.OrderByDescending(cm => cm.Student.Email) : query.OrderBy(cm => cm.Student.Email);
					break;
				case "Role":
					query = descending ? query.OrderByDescending(cm => cm.Student.Role) : query.OrderBy(cm => cm.Student.Role);
					break;
				case "CreatedAt":
					query = descending ? query.OrderByDescending(cm => cm.CreatedAt) : query.OrderBy(cm => cm.CreatedAt);
					break;
				default:
					query = descending ? query.OrderByDescending(cm => cm.Student.FullName) : query.OrderBy(cm => cm.Student.FullName);
					break;
			}

			var total = await query.CountAsync();

			var items = await query
				.Skip((pageNumber - 1) * pageSize)
				.Take(pageSize)
				.AsNoTracking()
				.ToListAsync();

			return (items, total);
		}
	}
}
