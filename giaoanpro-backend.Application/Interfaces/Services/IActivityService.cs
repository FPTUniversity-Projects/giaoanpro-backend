using giaoanpro_backend.Application.DTOs.Requests.Activities;
using giaoanpro_backend.Application.DTOs.Responses.Activities;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace giaoanpro_backend.Application.Interfaces.Services
{
    public interface IActivityService
    {
        Task<BaseResponse<PagedResult<ActivityResponse>>> GetActivitiesAsync(GetActivitiesQuery query);
        Task<BaseResponse<ActivityResponse>> GetActivityByIdAsync(Guid id);
        Task<BaseResponse<ActivityResponse>> CreateActivityAsync(CreateActivityRequest request);
        Task<BaseResponse<ActivityResponse>> UpdateActivityAsync(Guid id, UpdateActivityRequest request);
        Task<BaseResponse> DeleteActivityAsync(Guid id);
    }
}
