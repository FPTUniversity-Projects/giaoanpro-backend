using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface IClassRepository : IGenericRepository<Class>
	{
		Task<bool> ExistsByNameAndSemesterAsync(string name, Guid semesterId, Guid? excludeId = null);
		Task<IEnumerable<Class>> GetByTeacherIdAsync(Guid teacherId);
		Task<IEnumerable<Class>> GetBySemesterIdAsync(Guid semesterId);
		Task<IEnumerable<Class>> GetByGradeIdAsync(Guid gradeId);
		Task<Class?> GetWithMembersAsync(Guid classId);

		/// <summary>
		/// Get paged members of a class with optional search, role filter, sorting and paging performed in database.
		/// Returns items and total count.
		/// </summary>
		Task<(IEnumerable<ClassMember> Items, int TotalCount)> GetMembersPagedAsync(
			Guid classId,
			string? search = null,
			string? role = null,
			string? sortBy = null,
			bool descending = false,
			int pageNumber = 1,
			int pageSize = 10
		);
	}
}
