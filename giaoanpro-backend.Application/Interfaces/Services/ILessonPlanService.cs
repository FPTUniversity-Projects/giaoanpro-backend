using giaoanpro_backend.Application.DTOs.Requests.LessonPlans;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.LessonPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace giaoanpro_backend.Application.Interfaces.Services
{
    public interface ILessonPlanService
    {
        Task<BaseResponse<PagedResult<LessonPlanResponse>>> GetLessonPlansAsync(GetLessonPlansQuery query, Guid userId);
        Task<BaseResponse<LessonPlanResponse>> GetLessonPlanByIdAsync(Guid id);
        Task<BaseResponse<LessonPlanResponse>> CreateLessonPlanAsync(CreateLessonPlanRequest request, Guid userId);
        Task<BaseResponse<LessonPlanResponse>> UpdateLessonPlanAsync(Guid id, UpdateLessonPlanRequest request, Guid userId);
        Task<BaseResponse> DeleteLessonPlanAsync(Guid id, Guid userId);
    }
}
