using giaoanpro_backend.Application.DTOs.Requests.Materials;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Materials;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface IMaterialService
	{
		Task<BaseResponse<MaterialResponse>> UploadMaterialAsync(UploadMaterialRequest request, Guid userId);
		Task<BaseResponse> DeleteMaterialAsync(Guid id, Guid userId);
		Task<BaseResponse<List<MaterialResponse>>> GetMaterialsByActivityIdAsync(Guid activityId);
	}
}
