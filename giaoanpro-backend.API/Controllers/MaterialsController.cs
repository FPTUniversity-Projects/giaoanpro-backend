using giaoanpro_backend.Application.DTOs.Requests.Materials;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[Authorize]
	public class MaterialsController : BaseApiController
	{
		private readonly IMaterialService _materialService;

		public MaterialsController(IMaterialService materialService)
		{
			_materialService = materialService;
		}

		/// <summary>
		/// Upload material for an activity (Teacher only)
		/// </summary>
		[HttpPost("upload")]
		[Authorize(Roles = "Teacher")]
		public async Task<IActionResult> UploadMaterial([FromForm] UploadMaterialRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var userId = GetCurrentUserId();
			var result = await _materialService.UploadMaterialAsync(request, userId);
			return HandleResponse(result);
		}

		/// <summary>
		/// Delete material by ID (Teacher only)
		/// </summary>
		[HttpDelete("{id}")]
		[Authorize(Roles = "Teacher")]
		public async Task<IActionResult> DeleteMaterial(Guid id)
		{
			var userId = GetCurrentUserId();
			var result = await _materialService.DeleteMaterialAsync(id, userId);
			return HandleResponse(result);
		}

		/// <summary>
		/// Get all materials for an activity
		/// </summary>
		[HttpGet("activity/{activityId}")]
		public async Task<IActionResult> GetMaterialsByActivityId(Guid activityId)
		{
			var result = await _materialService.GetMaterialsByActivityIdAsync(activityId);
			return HandleResponse(result);
		}
	}
}
