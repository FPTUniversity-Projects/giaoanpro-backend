using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace giaoanpro_backend.Application.Services
{
    public class PdfService : IPdfService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PdfService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            // Set QuestPDF license for community use
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateLessonPlanPdfAsync(Guid lessonPlanId)
        {
            // Get lesson plan with all activities (including nested children)
            var lessonPlan = await _unitOfWork.LessonPlans.GetByConditionAsync(
                lp => lp.Id == lessonPlanId,
                include: q => q
                    .Include(lp => lp.Subject)
                        .ThenInclude(s => s.Grade)
                    .Include(lp => lp.User)
                    .Include(lp => lp.Activities.OrderBy(a => a.CreatedAt))
                        .ThenInclude(a => a.Children.OrderBy(c => c.CreatedAt)),
                asNoTracking: true
            );

            if (lessonPlan == null)
                throw new InvalidOperationException("Lesson plan not found");

            // Generate PDF using QuestPDF
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header()
                        .Text($"LESSON PLAN: {lessonPlan.Title}")
                        .FontSize(16)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(10);

                            // Lesson Plan Details
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text(text =>
                                    {
                                        text.Span("Teacher: ").Bold();
                                        text.Span(lessonPlan.User.FullName);
                                    });
                                    col.Item().Text(text =>
                                    {
                                        text.Span("Subject: ").Bold();
                                        text.Span(lessonPlan.Subject.Name);
                                    });
                                    col.Item().Text(text =>
                                    {
                                        text.Span("Grade: ").Bold();
                                        text.Span($"Grade {lessonPlan.Subject.Grade.Level}");
                                    });
                                });
                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text(text =>
                                    {
                                        text.Span("Created: ").Bold();
                                        text.Span(lessonPlan.CreatedAt.ToString("MMM dd, yyyy"));
                                    });
                                    col.Item().Text(text =>
                                    {
                                        text.Span("Updated: ").Bold();
                                        text.Span(lessonPlan.UpdatedAt.ToString("MMM dd, yyyy"));
                                    });
                                });
                            });

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Objective Section
                            if (!string.IsNullOrWhiteSpace(lessonPlan.Objective))
                            {
                                column.Item().Column(col =>
                                {
                                    col.Item().Text("OBJECTIVE").FontSize(14).Bold().FontColor(Colors.Blue.Darken1);
                                    col.Item().PaddingTop(5).Text(lessonPlan.Objective);
                                });
                            }

                            // Note Section
                            if (!string.IsNullOrWhiteSpace(lessonPlan.Note))
                            {
                                column.Item().Column(col =>
                                {
                                    col.Item().Text("NOTES").FontSize(14).Bold().FontColor(Colors.Blue.Darken1);
                                    col.Item().PaddingTop(5).Text(lessonPlan.Note);
                                });
                            }

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Activities Section
                            column.Item().Text("ACTIVITIES").FontSize(14).Bold().FontColor(Colors.Blue.Darken1);

                            var parentActivities = lessonPlan.Activities.Where(a => a.ParentId == null).ToList();
                            
                            if (parentActivities.Any())
                            {
                                foreach (var activity in parentActivities)
                                {
                                    column.Item().Element(c => RenderActivity(c, activity, 0));
                                }
                            }
                            else
                            {
                                column.Item().Text("No activities available").Italic().FontColor(Colors.Grey.Medium);
                            }
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();

            return pdfBytes;
        }

        private void RenderActivity(IContainer container, Domain.Entities.Activity activity, int level)
        {
            container.Column(column =>
            {
                column.Spacing(5);

                var indent = level * 20;
                
                // Activity Header
                column.Item().PaddingLeft(indent).Background(Colors.Grey.Lighten3).Padding(8).Column(col =>
                {
                    col.Item().Text(text =>
                    {
                        text.Span($"Activity: ").Bold().FontSize(12);
                        text.Span(activity.Title).FontSize(12);
                    });
                    
                    col.Item().Text(text =>
                    {
                        text.Span("Type: ").Bold().FontSize(10);
                        text.Span(activity.Type.ToString()).FontSize(10);
                    });
                });

                // Activity Details
                column.Item().PaddingLeft(indent + 10).Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(activity.Objective))
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Objective: ").Bold();
                            text.Span(activity.Objective);
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(activity.Content))
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Content: ").Bold();
                            text.Span(activity.Content);
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(activity.Product))
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Product: ").Bold();
                            text.Span(activity.Product);
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(activity.Implementation))
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Implementation: ").Bold();
                            text.Span(activity.Implementation);
                        });
                    }
                });

                // Render child activities recursively
                if (activity.Children != null && activity.Children.Any())
                {
                    foreach (var child in activity.Children.OrderBy(c => c.CreatedAt))
                    {
                        column.Item().Element(c => RenderActivity(c, child, level + 1));
                    }
                }
            });
        }
    }
}
