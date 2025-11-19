using giaoanpro_backend.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace giaoanpro_backend.Application.DTOs.Requests.Activities
{
    public class CreateActivityRequest
    {
        public Guid LessonPlanId { get; set; }
        public Guid? ParentId { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ActivityType Type { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Objective { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
        public string Implementation { get; set; } = string.Empty;
    }
}
