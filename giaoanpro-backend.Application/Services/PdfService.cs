using giaoanpro_backend.Application.Interfaces.Repositories.Bases;
using giaoanpro_backend.Application.Interfaces.Services;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.RegularExpressions;

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

                    // Header - appears on all pages with consistent formatting
                    page.Header().Column(column =>
                    {
                        column.Item().Text($"LESSON PLAN: {lessonPlan.Title}")
                            .FontSize(14)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);
                        
                        column.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
                    });

                    page.Content()
                        .PaddingVertical(0.5f, Unit.Centimetre)
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
                                    col.Item().Text("OBJECTIVE").FontSize(13).Bold().FontColor(Colors.Blue.Darken1);
                                    col.Item().PaddingTop(5).Element(c => RenderHtmlContent(c, lessonPlan.Objective));
                                });
                            }

                            // Note Section
                            if (!string.IsNullOrWhiteSpace(lessonPlan.Note))
                            {
                                column.Item().Column(col =>
                                {
                                    col.Item().Text("NOTES").FontSize(13).Bold().FontColor(Colors.Blue.Darken1);
                                    col.Item().PaddingTop(5).Element(c => RenderHtmlContent(c, lessonPlan.Note));
                                });
                            }

                            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                            // Activities Section
                            column.Item().Text("ACTIVITIES").FontSize(13).Bold().FontColor(Colors.Blue.Darken1);

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
                        col.Item().Column(contentCol =>
                        {
                            contentCol.Item().Text("Objective: ").Bold();
                            contentCol.Item().PaddingLeft(10).Element(c => RenderHtmlContent(c, activity.Objective));
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(activity.Content))
                    {
                        col.Item().Column(contentCol =>
                        {
                            contentCol.Item().Text("Content: ").Bold();
                            contentCol.Item().PaddingLeft(10).Element(c => RenderHtmlContent(c, activity.Content));
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(activity.Product))
                    {
                        col.Item().Column(contentCol =>
                        {
                            contentCol.Item().Text("Product: ").Bold();
                            contentCol.Item().PaddingLeft(10).Element(c => RenderHtmlContent(c, activity.Product));
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(activity.Implementation))
                    {
                        col.Item().Column(contentCol =>
                        {
                            contentCol.Item().Text("Implementation: ").Bold();
                            contentCol.Item().PaddingLeft(10).Element(c => RenderHtmlContent(c, activity.Implementation));
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

        /// <summary>
        /// Renders HTML content by parsing and formatting it properly
        /// </summary>
        private void RenderHtmlContent(IContainer container, string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
                return;

            // Check if content contains HTML tags
            if (!htmlContent.Contains('<'))
            {
                // Plain text - render as is
                container.Text(htmlContent);
                return;
            }

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                container.Column(column =>
                {
                    ProcessHtmlNode(column, doc.DocumentNode);
                });
            }
            catch
            {
                // If HTML parsing fails, render as plain text (strip tags)
                container.Text(StripHtmlTags(htmlContent));
            }
        }

        /// <summary>
        /// Recursively processes HTML nodes and renders them
        /// </summary>
        private void ProcessHtmlNode(ColumnDescriptor column, HtmlNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                switch (child.NodeType)
                {
                    case HtmlNodeType.Text:
                        var text = HtmlEntity.DeEntitize(child.InnerText).Trim();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            column.Item().Text(text);
                        }
                        break;

                    case HtmlNodeType.Element:
                        ProcessHtmlElement(column, child);
                        break;
                }
            }
        }

        /// <summary>
        /// Processes specific HTML elements
        /// </summary>
        private void ProcessHtmlElement(ColumnDescriptor column, HtmlNode element)
        {
            var tagName = element.Name.ToLower();

            switch (tagName)
            {
                case "p":
                    var pText = GetInnerTextWithFormatting(element);
                    if (!string.IsNullOrWhiteSpace(pText))
                    {
                        column.Item().PaddingBottom(5).Text(text => RenderFormattedText(text, element));
                    }
                    break;

                case "ol":
                    column.Item().PaddingLeft(15).Column(listCol =>
                    {
                        int counter = 1;
                        foreach (var li in element.SelectNodes(".//li") ?? Enumerable.Empty<HtmlNode>())
                        {
                            listCol.Item().Row(row =>
                            {
                                row.ConstantItem(30).Text($"{counter}.");
                                row.RelativeItem().Column(itemCol =>
                                {
                                    ProcessHtmlNode(itemCol, li);
                                });
                            });
                            counter++;
                        }
                    });
                    break;

                case "ul":
                    column.Item().PaddingLeft(15).Column(listCol =>
                    {
                        foreach (var li in element.SelectNodes(".//li") ?? Enumerable.Empty<HtmlNode>())
                        {
                            listCol.Item().Row(row =>
                            {
                                row.ConstantItem(30).Text("•");
                                row.RelativeItem().Column(itemCol =>
                                {
                                    ProcessHtmlNode(itemCol, li);
                                });
                            });
                        }
                    });
                    break;

                case "li":
                    // Handled by ol/ul parent
                    break;

                case "strong":
                case "b":
                    var strongText = GetInnerTextWithFormatting(element);
                    if (!string.IsNullOrWhiteSpace(strongText))
                    {
                        column.Item().Text(strongText).Bold();
                    }
                    break;

                case "em":
                case "i":
                    var emText = GetInnerTextWithFormatting(element);
                    if (!string.IsNullOrWhiteSpace(emText))
                    {
                        column.Item().Text(emText).Italic();
                    }
                    break;

                case "blockquote":
                    column.Item().PaddingLeft(20).BorderLeft(3).BorderColor(Colors.Grey.Medium).PaddingLeft(10)
                        .Column(quoteCol =>
                        {
                            ProcessHtmlNode(quoteCol, element);
                        });
                    break;

                case "br":
                    column.Item().PaddingBottom(5);
                    break;

                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    var headerText = GetInnerTextWithFormatting(element);
                    if (!string.IsNullOrWhiteSpace(headerText))
                    {
                        var fontSize = tagName switch
                        {
                            "h1" => 18,
                            "h2" => 16,
                            "h3" => 14,
                            "h4" => 13,
                            "h5" => 12,
                            _ => 11
                        };
                        column.Item().PaddingVertical(5).Text(headerText).FontSize(fontSize).Bold();
                    }
                    break;

                default:
                    // For unhandled elements, process their children
                    ProcessHtmlNode(column, element);
                    break;
            }
        }

        /// <summary>
        /// Renders formatted text with inline styles (bold, italic, etc.)
        /// </summary>
        private void RenderFormattedText(TextDescriptor text, HtmlNode node)
        {
            foreach (var child in node.ChildNodes)
            {
                if (child.NodeType == HtmlNodeType.Text)
                {
                    var content = HtmlEntity.DeEntitize(child.InnerText).Trim();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        text.Span(content);
                    }
                }
                else if (child.NodeType == HtmlNodeType.Element)
                {
                    var tagName = child.Name.ToLower();
                    var content = GetInnerTextWithFormatting(child);

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        switch (tagName)
                        {
                            case "strong":
                            case "b":
                                text.Span(content).Bold();
                                break;
                            case "em":
                            case "i":
                                text.Span(content).Italic();
                                break;
                            default:
                                text.Span(content);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets inner text while preserving formatting structure
        /// </summary>
        private string GetInnerTextWithFormatting(HtmlNode node)
        {
            return HtmlEntity.DeEntitize(node.InnerText).Trim();
        }

        /// <summary>
        /// Strips HTML tags and returns plain text
        /// </summary>
        private string StripHtmlTags(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Remove HTML tags
            var text = Regex.Replace(html, "<.*?>", string.Empty);
            // Decode HTML entities
            text = HtmlEntity.DeEntitize(text);
            return text.Trim();
        }
    }
}
