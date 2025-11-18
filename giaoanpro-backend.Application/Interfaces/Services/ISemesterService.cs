using giaoanpro_backend.Application.DTOs.Requests.Semesters;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Semesters;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface ISemesterService
	{
		Task<BaseResponse<PagedResult<SemesterResponse>>> GetSemestersAsync(GetSemestersQuery query);
		Task<BaseResponse<SemesterResponse>> GetSemesterByIdAsync(Guid id);
		Task<BaseResponse<SemesterResponse>> CreateSemesterAsync(CreateSemesterRequest request);
		Task<BaseResponse<SemesterResponse>> UpdateSemesterAsync(Guid id, UpdateSemesterRequest request);
		Task<BaseResponse> DeleteSemesterAsync(Guid id);
	}
}
