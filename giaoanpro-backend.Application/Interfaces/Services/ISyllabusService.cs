using giaoanpro_backend.Application.DTOs.Requests.Syllabuses;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Syllabuses;

namespace giaoanpro_backend.Application.Interfaces.Services
{
	public interface ISyllabusService
	{
		Task<BaseResponse<PagedResult<SyllabusResponse>>> GetSyllabusesAsync(GetSyllabusesQuery query);
		Task<BaseResponse<SyllabusResponse>> GetSyllabusByIdAsync(Guid id);
		Task<BaseResponse<SyllabusResponse>> GetSyllabusBySubjectIdAsync(Guid subjectId);
		Task<BaseResponse<SyllabusResponse>> CreateSyllabusAsync(CreateSyllabusRequest request);
		Task<BaseResponse<SyllabusResponse>> UpdateSyllabusAsync(Guid id, UpdateSyllabusRequest request);
		Task<BaseResponse> DeleteSyllabusAsync(Guid id);
	}
}
