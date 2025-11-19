using System;
using System.Collections.Generic;
using System.Linq;
using Mapster;
using giaoanpro_backend.Application.DTOs.Requests.Exams;
using giaoanpro_backend.Application.DTOs.Responses.Exams;
using giaoanpro_backend.Domain.Entities;
using giaoanpro_backend.Application.DTOs.Responses.Questions;

namespace giaoanpro_backend.Application.Mappings
{
    public class ExamRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            // CreateExamRequest -> Exam
            config.NewConfig<CreateExamRequest, Exam>()
                .Map(dest => dest.Id, src => Guid.NewGuid())
                .Map(dest => dest.CreatedAt, src => DateTime.UtcNow)
                .Map(dest => dest.UpdatedAt, src => DateTime.UtcNow)
                .Map(dest => dest.ActivityId, src => (Guid?)null)
                .AfterMapping((src, dest) =>
                {
                    if (src.QuestionIds != null && src.QuestionIds.Count > 0)
                    {
                        var now = DateTime.UtcNow;
                        var seq = 1;
                        foreach (var qid in src.QuestionIds)
                        {
                            dest.ExamQuestions.Add(new ExamQuestion
                            {
                                ExamId = dest.Id,
                                QuestionId = qid,
                                SequenceNumber = seq++,
                                CreatedAt = now,
                                UpdatedAt = now
                            });
                        }
                    }
                });

            // Exam -> GetExamDetailResponse
            var examToDetail = config.NewConfig<Exam, GetExamDetailResponse>();
            examToDetail
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.DurationMinutes, src => src.DurationMinutes)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.ActivityId, src => src.ActivityId);

            examToDetail.AfterMapping((src, dest) =>
            {
                if (src.ExamQuestions == null)
                {
                    dest.Questions = new List<GetQuestionResponse>();
                    return;
                }

                dest.Questions = src.ExamQuestions
                    .OrderBy(eq => eq.SequenceNumber)
                    .Select(eq => eq.Question.Adapt<GetQuestionResponse>())
                    .ToList();
            });

            // Exam -> ExamSummaryResponse
            config.NewConfig<Exam, ExamSummaryResponse>()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.DurationMinutes, src => src.DurationMinutes)
                .Map(dest => dest.ActivityId, src => src.ActivityId)
                .Map(dest => dest.QuestionCount, src => src.ExamQuestions == null ? 0 : src.ExamQuestions.Count);
        }
    }
}
