using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Domain.Entities;

namespace giaoanpro_backend.Application.Interfaces.Repositories
{
	public interface IClassMemberRepository : IGenericRepository<ClassMember>
	{
		/// <summary>
		/// Get paged ClassMember records for a specific student with optional class filters.
		/// Returns items and total count.
		/// </summary>
		Task<(IEnumerable<ClassMember> Items, int TotalCount)> GetPagedByStudentAsync(
			Guid studentId,
			string? className = null,
			Guid? teacherId = null,
			Guid? gradeId = null,
			Guid? semesterId = null,
			int pageNumber = 1,
			int pageSize = 10
		);
	}
}
