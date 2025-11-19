using giaoanpro_backend.Application.DTOs.Requests.Subjects;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Subjects;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface ISubjectService
	{
		Task<BaseResponse<PagedResult<SubjectResponse>>> GetSubjectsAsync(GetSubjectsQuery query);
		Task<BaseResponse<SubjectResponse>> GetSubjectByIdAsync(Guid id);
		Task<BaseResponse<SubjectResponse>> CreateSubjectAsync(CreateSubjectRequest request);
		Task<BaseResponse<SubjectResponse>> UpdateSubjectAsync(Guid id, UpdateSubjectRequest request);
		Task<BaseResponse> DeleteSubjectAsync(Guid id);
	}
}
