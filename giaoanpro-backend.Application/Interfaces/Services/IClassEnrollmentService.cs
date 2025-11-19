using giaoanpro_backend.Application.DTOs.Requests.Classes;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Classes;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface IClassEnrollmentService
	{
		Task<BaseResponse<PagedResult<ClassResponse>>> GetEnrolledClassesByStudentIdAsync(Guid studentId, GetEnrolledClassQuery query);
		Task<BaseResponse<string>> EnrollStudentToClassAsync(Guid classId, Guid studentId);
		Task<BaseResponse<string>> RemoveStudentFromClassAsync(Guid classId, Guid studentId);
	}
}
