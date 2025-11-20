using System;
using System.IO;
using giaoanpro_backend.Application.DTOs.Requests.Syllabuses;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace giaoanpro_backend.API.Controllers
{
	[Route("api/[controller]")]
	[Authorize]
	public class SyllabusesController : BaseApiController
	{
		private readonly ISyllabusService _syllabusService;
		private readonly IWebHostEnvironment _env;

		public SyllabusesController(ISyllabusService syllabusService, IWebHostEnvironment env)
		{
			_syllabusService = syllabusService;
			_env = env;
		}

		[HttpGet]
		public async Task<IActionResult> GetSyllabuses([FromQuery] GetSyllabusesQuery query)
		{
			var result = await _syllabusService.GetSyllabusesAsync(query);
			return HandleResponse(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetSyllabusById(Guid id)
		{
			var result = await _syllabusService.GetSyllabusByIdAsync(id);
			return HandleResponse(result);
		}

		[HttpGet("by-subject/{subjectId}")]
		public async Task<IActionResult> GetSyllabusBySubjectId(Guid subjectId)
		{
			var result = await _syllabusService.GetSyllabusBySubjectIdAsync(subjectId);
			return HandleResponse(result);
		}

		/// <summary>
		/// Create syllabus without file upload (JSON body)
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> CreateSyllabus([FromBody] CreateSyllabusRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _syllabusService.CreateSyllabusAsync(request);
			return HandleResponse(result);
		}

		/// <summary>
		/// Create syllabus with PDF file upload (multipart/form-data)
		/// </summary>
		[HttpPost("upload")]
		[Consumes("multipart/form-data")]
		[DisableRequestSizeLimit]
		public async Task<IActionResult> CreateSyllabusWithFile(
			[FromForm] Guid SubjectId,
			[FromForm] string Name,
			[FromForm] string Description,
			IFormFile file)
		{
			// Validate file
			if (file == null || file.Length == 0)
			{
				return BadRequest(BaseResponse.Fail("PDF file is required", ResponseErrorType.BadRequest));
			}

			if (!string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
			{
				return BadRequest(BaseResponse.Fail("Only PDF files are allowed.", ResponseErrorType.BadRequest));
			}

			const long maxFileSize = 10 * 1024 * 1024; // 10 MB
			if (file.Length > maxFileSize)
			{
				return BadRequest(BaseResponse.Fail("File size exceeds the limit of 10 MB.", ResponseErrorType.BadRequest));
			}

			// Save file to wwwroot/uploads/syllabuses
			var uploadsRoot = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "syllabuses");
			Directory.CreateDirectory(uploadsRoot);

			var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
			var filePath = Path.Combine(uploadsRoot, fileName);

			await using (var stream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			// Build accessible URL
			var url = $"{Request.Scheme}://{Request.Host}/uploads/syllabuses/{fileName}";

			// Create syllabus with PDF URL
			var request = new CreateSyllabusRequest
			{
				SubjectId = SubjectId,
				Name = Name,
				Description = Description,
				PdfUrl = url
			};

			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _syllabusService.CreateSyllabusAsync(request);
			return HandleResponse(result);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateSyllabus(Guid id, [FromBody] UpdateSyllabusRequest request)
		{
			var validation = ValidateRequestBody(request);
			if (validation != null) return validation;

			var result = await _syllabusService.UpdateSyllabusAsync(id, request);
			return HandleResponse(result);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteSyllabus(Guid id)
		{
			var result = await _syllabusService.DeleteSyllabusAsync(id);
			return HandleResponse(result);
		}
	}
}
