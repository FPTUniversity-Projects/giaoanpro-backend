using System;
using System.Threading.Tasks;
using giaoanpro_backend.Application.DTOs.Requests.ExamMatrices;
using giaoanpro_backend.Application.DTOs.Responses.ExamMatrices;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using System.Collections.Generic;

namespace giaoanpro_backend.Application.Interfaces.Services
{
    public interface IExamMatrixService
    {
        Task<BaseResponse<GetMatrixResponse>> CreateMatrixAsync(CreateMatrixRequest request);
        Task<BaseResponse<GetMatrixDetailResponse>> GetMatrixByIdAsync(Guid id);
        Task<BaseResponse<GetMatrixPagedResponse>> GetMatricesPagedAsync(GetMatrixPagedRequest request);
        Task<BaseResponse<string>> UpdateMatrixAsync(UpdateMatrixRequest request);
        Task<BaseResponse<string>> DeleteMatrixAsync(Guid id);
    }
}
