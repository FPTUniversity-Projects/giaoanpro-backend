using giaoanpro_backend.Application.DTOs.Requests.Grades;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Grades;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface IGradeService
	{
		Task<BaseResponse<PagedResult<GradeResponse>>> GetGradesAsync(GetGradesQuery query);
		Task<BaseResponse<GradeResponse>> GetGradeByIdAsync(Guid id);
		Task<BaseResponse<GradeResponse>> CreateGradeAsync(CreateGradeRequest request);
		Task<BaseResponse<GradeResponse>> UpdateGradeAsync(Guid id, UpdateGradeRequest request);
		Task<BaseResponse> DeleteGradeAsync(Guid id);
	}
}
