using giaoanpro_backend.Application.DTOs.Requests.Materials;
using giaoanpro_backend.Application.DTOs.Responses.Bases;
using giaoanpro_backend.Application.DTOs.Responses.Materials;
using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace giaoanpro_backend.Application.Services
{
	public class MaterialService : IMaterialService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IS3Service _s3Service;

		private readonly Dictionary<MaterialType, string[]> _allowedExtensions = new()
		{
			{ MaterialType.Document, new[] { ".doc", ".docx", ".txt", ".rtf" } },
			{ MaterialType.PDF, new[] { ".pdf" } },
			{ MaterialType.Excel, new[] { ".xls", ".xlsx", ".csv" } },
			{ MaterialType.Image, new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg" } },
			{ MaterialType.PowerPoint, new[] { ".ppt", ".pptx" } },
			{ MaterialType.Video, new[] { ".mp4", ".avi", ".mov", ".wmv", ".flv" } },
			{ MaterialType.Audio, new[] { ".mp3", ".wav", ".ogg", ".m4a" } },
			{ MaterialType.Other, new[] { ".zip", ".rar", ".7z" } }
		};

		// Map file extensions to proper content types
		private readonly Dictionary<string, string> _extensionToContentType = new()
		{
			// Documents
			{ ".doc", "application/msword" },
			{ ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
			{ ".txt", "text/plain" },
			{ ".rtf", "application/rtf" },
			// PDF
			{ ".pdf", "application/pdf" },
			// Excel
			{ ".xls", "application/vnd.ms-excel" },
			{ ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
			{ ".csv", "text/csv" },
			// Images
			{ ".jpg", "image/jpeg" },
			{ ".jpeg", "image/jpeg" },
			{ ".png", "image/png" },
			{ ".gif", "image/gif" },
			{ ".bmp", "image/bmp" },
			{ ".svg", "image/svg+xml" },
			// PowerPoint
			{ ".ppt", "application/vnd.ms-powerpoint" },
			{ ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
			// Video
			{ ".mp4", "video/mp4" },
			{ ".avi", "video/x-msvideo" },
			{ ".mov", "video/quicktime" },
			{ ".wmv", "video/x-ms-wmv" },
			{ ".flv", "video/x-flv" },
			// Audio
			{ ".mp3", "audio/mpeg" },
			{ ".wav", "audio/wav" },
			{ ".ogg", "audio/ogg" },
			{ ".m4a", "audio/mp4" },
			// Archives
			{ ".zip", "application/zip" },
			{ ".rar", "application/x-rar-compressed" },
			{ ".7z", "application/x-7z-compressed" }
		};

		public MaterialService(IUnitOfWork unitOfWork, IS3Service s3Service)
		{
			_unitOfWork = unitOfWork;
			_s3Service = s3Service;
		}

		public async Task<BaseResponse<MaterialResponse>> UploadMaterialAsync(UploadMaterialRequest request, Guid userId)
		{
			try
			{
				// Validate request
				if (request == null)
				{
					return BaseResponse<MaterialResponse>.Fail("Request cannot be null", ResponseErrorType.BadRequest);
				}

				if (request.File == null)
				{
					return BaseResponse<MaterialResponse>.Fail("File cannot be null", ResponseErrorType.BadRequest);
				}

				if (string.IsNullOrWhiteSpace(request.Title))
				{
					return BaseResponse<MaterialResponse>.Fail("Title cannot be empty", ResponseErrorType.BadRequest);
				}

				// Check if activity exists
				var activity = await _unitOfWork.Activities.GetByConditionAsync(
					a => a.Id == request.ActivityId,
					include: q => q.Include(a => a.LessonPlan)
				);

				if (activity == null)
				{
					return BaseResponse<MaterialResponse>.Fail("Activity not found", ResponseErrorType.NotFound);
				}

				// Check ownership
				if (activity.LessonPlan.UserId != userId)
				{
					return BaseResponse<MaterialResponse>.Fail(
						"You don't have permission to upload materials to this activity",
						ResponseErrorType.Forbidden);
				}

				// Validate file extension
				var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
				
				if (string.IsNullOrWhiteSpace(fileExtension))
				{
					return BaseResponse<MaterialResponse>.Fail(
						"File must have an extension",
						ResponseErrorType.BadRequest);
				}

				if (!_allowedExtensions.TryGetValue(request.Type, out var allowedExts) ||
					!allowedExts.Contains(fileExtension))
				{
					return BaseResponse<MaterialResponse>.Fail(
						$"File extension '{fileExtension}' is not allowed for material type '{request.Type}'. " +
						$"Allowed extensions: {string.Join(", ", allowedExts)}",
						ResponseErrorType.BadRequest);
				}

				// Validate file size (max 50MB)
				const long maxFileSize = 50 * 1024 * 1024; // 50MB
				if (request.File.Length > maxFileSize)
				{
					return BaseResponse<MaterialResponse>.Fail(
						$"File size exceeds maximum allowed size of 50MB",
						ResponseErrorType.BadRequest);
				}

				if (request.File.Length == 0)
				{
					return BaseResponse<MaterialResponse>.Fail(
						"File is empty",
						ResponseErrorType.BadRequest);
				}

				// Read file content
				byte[] fileContent;
				using (var memoryStream = new MemoryStream())
				{
					await request.File.CopyToAsync(memoryStream);
					fileContent = memoryStream.ToArray();
				}

				if (fileContent == null || fileContent.Length == 0)
				{
					return BaseResponse<MaterialResponse>.Fail(
						"Failed to read file content",
						ResponseErrorType.BadRequest);
				}

				// Get content type based on extension
				var contentType = _extensionToContentType.TryGetValue(fileExtension, out var ct) 
					? ct 
					: "application/octet-stream";

				// Upload to S3
				var fileUrl = await _s3Service.UploadFileAsync(request.File.FileName, fileContent, contentType);

				if (string.IsNullOrWhiteSpace(fileUrl))
				{
					return BaseResponse<MaterialResponse>.Fail(
						"Failed to upload file to S3",
						ResponseErrorType.InternalError);
				}

				// Create material entity
				var material = new Material
				{
					Id = Guid.NewGuid(),
					ActivityId = request.ActivityId,
					Title = request.Title,
					Type = request.Type,
					Url = fileUrl
				};

				await _unitOfWork.Materials.AddAsync(material);
				await _unitOfWork.SaveChangesAsync();

				var response = new MaterialResponse
				{
					Id = material.Id,
					ActivityId = material.ActivityId,
					Title = material.Title,
					Type = material.Type,
					Url = material.Url,
					CreatedAt = material.CreatedAt,
					UpdatedAt = material.UpdatedAt
				};

				return BaseResponse<MaterialResponse>.Ok(response, "Material uploaded successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse<MaterialResponse>.Fail($"Error uploading material: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse> DeleteMaterialAsync(Guid id, Guid userId)
		{
			try
			{
				var material = await _unitOfWork.Materials.GetByConditionAsync(
					m => m.Id == id,
					include: q => q.Include(m => m.Activity)
						.ThenInclude(a => a.LessonPlan)
				);

				if (material == null)
				{
					return BaseResponse.Fail("Material not found", ResponseErrorType.NotFound);
				}

				// Check ownership
				if (material.Activity.LessonPlan.UserId != userId)
				{
					return BaseResponse.Fail(
						"You don't have permission to delete this material",
						ResponseErrorType.Forbidden);
				}

				// Delete from S3
				try
				{
					await _s3Service.DeleteFileAsync(material.Url);
				}
				catch (Exception ex)
				{
					// Log error but continue with database deletion
					Console.WriteLine($"Error deleting file from S3: {ex.Message}");
				}

				// Delete from database
				_unitOfWork.Materials.Remove(material);
				await _unitOfWork.SaveChangesAsync();

				return BaseResponse.Ok("Material deleted successfully");
			}
			catch (Exception ex)
			{
				return BaseResponse.Fail($"Error deleting material: {ex.Message}", ResponseErrorType.InternalError);
			}
		}

		public async Task<BaseResponse<List<MaterialResponse>>> GetMaterialsByActivityIdAsync(Guid activityId)
		{
			try
			{
				var materials = await _unitOfWork.Materials.GetByActivityIdAsync(activityId);

				var response = materials.Select(m => new MaterialResponse
				{
					Id = m.Id,
					ActivityId = m.ActivityId,
					Title = m.Title,
					Type = m.Type,
					Url = m.Url,
					CreatedAt = m.CreatedAt,
					UpdatedAt = m.UpdatedAt
				}).ToList();

				return BaseResponse<List<MaterialResponse>>.Ok(response);
			}
			catch (Exception ex)
			{
				return BaseResponse<List<MaterialResponse>>.Fail($"Error retrieving materials: {ex.Message}", ResponseErrorType.InternalError);
			}
		}
	}
}
