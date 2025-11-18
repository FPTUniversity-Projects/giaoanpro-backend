using giaoanpro_backend.Application.DTOs.Requests.Classes;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Classes;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface IClassService
	{
		Task<BaseResponse<PagedResult<ClassResponse>>> GetClassesAsync(GetClassesQuery query);
		Task<BaseResponse<ClassResponse>> GetClassByIdAsync(Guid id);
		Task<BaseResponse<ClassResponse>> CreateClassAsync(CreateClassRequest request);
		Task<BaseResponse<ClassResponse>> UpdateClassAsync(Guid id, UpdateClassRequest request);
		Task<BaseResponse> DeleteClassAsync(Guid id);
	}
}
