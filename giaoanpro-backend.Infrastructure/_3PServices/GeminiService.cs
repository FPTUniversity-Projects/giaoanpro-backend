using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using giaoanpro_backend.Application.DTOs.Requests.Questions;
using giaoanpro_backend.Application.Interfaces.Services._3PServices;
using Microsoft.Extensions.Configuration;

namespace giaoanpro_backend.Infrastructure._3PServices
{
    public class GeminiService : IGeminiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public GeminiService(IHttpClientFactory httpClientFactory, IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task<string> GenerateQuestionsJsonAsync(Guid lessonPlanId, string context, GenerateQuestionSpec spec, int count, CancellationToken ct = default)
        {
            var apiKey = _config["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Missing Gemini API key (Gemini:ApiKey or GOOGLE_API_KEY).");
            }

            // Lời nhắc sinh câu hỏi bằng tiếng Việt, bám sát nội dung LessonPlan đã join (context)
            var prompt = $@"
                Bạn là chuyên gia tạo câu hỏi Ngữ văn bằng tiếng Việt.
                Hãy dựa TRỰC TIẾP vào nội dung KẾ HOẠCH BÀI DẠY và CÁC HOẠT ĐỘNG dưới đây để tạo {count} câu hỏi trắc nghiệm về NGỮ VĂN (không được ngoài phạm vi nội dung):
                Nội dung kế hoạch bài dạy và hoạt động:
                {context}
                Yêu cầu cho mỗi câu hỏi:
                - QuestionType: {spec.QuestionType}
                - DifficultyLevel: {spec.DifficultyLevel}
                - AwarenessLevel (Bloom): {spec.AwarenessLevel}
                - Văn phong, từ ngữ và đáp án đều phải bằng TIẾNG VIỆT, phù hợp học sinh phổ thông.
                - Mỗi câu hỏi phải có 4 phương án trả lời.
                - CHỈ có đúng 1 đáp án isCorrect=true.

                CHỈ TRẢ VỀ JSON THUẦN (không giải thích gì thêm) theo đúng cấu trúc sau:

                {{
                  ""questions"": [
                    {{
                      ""text"": ""string"",
                      ""options"": [
                        {{ ""text"": ""string"", ""isCorrect"": true }},
                        {{ ""text"": ""string"", ""isCorrect"": false }}
                      ]
                    }}
                  ]
                }}";

            var payload = new
            {
                contents = new[]
                {
                    new {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(60);
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            using var res = await client.SendAsync(req, ct);
            var body = await res.Content.ReadAsStringAsync(ct);
            if (!res.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Gemini API error: {(int)res.StatusCode} {res.ReasonPhrase} - {body}");
            }

            // Parse Gemini response to extract text (which should be JSON per our instruction)
            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("candidates", out var candidatesEl) || candidatesEl.ValueKind != JsonValueKind.Array || candidatesEl.GetArrayLength() == 0)
                throw new InvalidOperationException("Gemini returned no candidates.");
            var first = candidatesEl[0];
            if (!first.TryGetProperty("content", out var contentEl))
                throw new InvalidOperationException("Gemini response missing content.");
            if (!contentEl.TryGetProperty("parts", out var partsEl) || partsEl.ValueKind != JsonValueKind.Array || partsEl.GetArrayLength() == 0)
                throw new InvalidOperationException("Gemini response missing parts.");
            var text = partsEl[0].GetProperty("text").GetString();
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException("Gemini returned empty content.");

            var jsonStart = text.IndexOf('{');
            var jsonEnd = text.LastIndexOf('}');

            if (jsonStart == -1 || jsonEnd == -1 || jsonEnd < jsonStart)
            {
                throw new InvalidOperationException("Gemini response did not contain valid JSON.");
            }

            var jsonContent = text.Substring(jsonStart, jsonEnd - jsonStart + 1);
            return jsonContent;
        }

        public async Task<string> GenerateContentAsync(string prompt, CancellationToken ct = default)
        {
            var apiKey = _config["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("Missing Gemini API key (Gemini:ApiKey or GOOGLE_API_KEY).");
            }

            var client = _httpClientFactory.CreateClient("Gemini");
            client.Timeout = TimeSpan.FromSeconds(60);

            var body = new
            {
                model = "gemini-2.0-flash",
                response_format = new { type = "json_object" },
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful assistant." },
                    new { role = "user", content = prompt }
                }
            };

            var req = new HttpRequestMessage(HttpMethod.Post, "https://generativelanguage.googleapis.com/v1beta/openai/chat/completions")
            {
                Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
            };
            req.Headers.Add("Authorization", $"Bearer {apiKey}");

            using var res = await client.SendAsync(req, ct);
            var respBody = await res.Content.ReadAsStringAsync(ct);
            if (!res.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Gemini API error: {(int)res.StatusCode} {res.ReasonPhrase} - {respBody}");
            }

            using var doc = JsonDocument.Parse(respBody);

            // Try to find the content field in the response
            if (doc.RootElement.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
            {
                var first = choices[0];
                if (first.TryGetProperty("message", out var message) && message.TryGetProperty("content", out var contentEl))
                {
                    if (contentEl.ValueKind == JsonValueKind.String)
                    {
                        return contentEl.GetString() ?? string.Empty;
                    }
                    else if (contentEl.ValueKind == JsonValueKind.Object)
                    {
                        return contentEl.GetRawText();
                    }
                }
            }

            // fallback: return entire response body
            return respBody;
        }
    }
}
