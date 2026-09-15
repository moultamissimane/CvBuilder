using CVBuilder.API.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CVBuilder.API.Services
{
    public interface IPdfExportService
    {
        byte[] ExportToPdf(StructuredCv cv);
    }

    /// <summary>
    /// Renders a StructuredCv to a real, selectable-text A4 PDF. Deliberately single-column with no
    /// tables, icons, or graphics — that's what keeps it parsing cleanly through an ATS.
    /// </summary>
    public class PdfExportService : IPdfExportService
    {
        private static readonly BaseColor HeadingColor = new BaseColor(30, 58, 95);
        private static readonly BaseColor MutedColor = new BaseColor(90, 90, 90);
        private static readonly BaseColor RuleColor = new BaseColor(200, 200, 200);

        public byte[] ExportToPdf(StructuredCv cv)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 48, 48, 40, 40);
            PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            var nameFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, BaseColor.BLACK);
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, MutedColor);
            var contactFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, MutedColor);
            var headingFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, HeadingColor);
            var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
            var boldBodyFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BaseColor.BLACK);
            var mutedBodyFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9.5f, MutedColor);

            // Header
            document.Add(new Paragraph(cv.PersonalInfo.FullName ?? "", nameFont) { SpacingAfter = 2 });
            if (!string.IsNullOrWhiteSpace(cv.PersonalInfo.Title))
                document.Add(new Paragraph(cv.PersonalInfo.Title, titleFont) { SpacingAfter = 2 });
            if (!string.IsNullOrWhiteSpace(cv.PersonalInfo.ContactLine))
                document.Add(new Paragraph(cv.PersonalInfo.ContactLine, contactFont) { SpacingAfter = 8 });

            AddRule(document);

            if (!string.IsNullOrWhiteSpace(cv.Summary))
            {
                AddSectionHeading(document, "Professional Summary", headingFont);
                document.Add(new Paragraph(cv.Summary, bodyFont) { SpacingAfter = 10, Leading = 14 });
            }

            if (cv.Experience.Count > 0)
            {
                AddSectionHeading(document, "Professional Experience", headingFont);
                foreach (var exp in cv.Experience)
                {
                    var titleLine = new Paragraph { SpacingBefore = 6, SpacingAfter = 1 };
                    titleLine.Add(new Chunk($"{exp.JobTitle} — {exp.Company}", boldBodyFont));
                    document.Add(titleLine);

                    if (!string.IsNullOrWhiteSpace(exp.DateRange))
                        document.Add(new Paragraph(exp.DateRange, mutedBodyFont) { SpacingAfter = 3 });

                    if (exp.Technologies.Count > 0)
                        document.Add(new Paragraph($"Tools: {string.Join(", ", exp.Technologies)}", mutedBodyFont) { SpacingAfter = 3 });

                    foreach (var bullet in exp.Description)
                    {
                        var p = new Paragraph($"•  {bullet}", bodyFont) { SpacingAfter = 2, Leading = 13, IndentationLeft = 10 };
                        document.Add(p);
                    }
                }
                document.Add(new Paragraph(" ", bodyFont) { SpacingAfter = 4 });
            }
            else if (!string.IsNullOrWhiteSpace(cv.RawExperienceText))
            {
                AddSectionHeading(document, "Professional Experience", headingFont);
                document.Add(new Paragraph(cv.RawExperienceText, bodyFont) { SpacingAfter = 10, Leading = 14 });
            }

            if (cv.Education.Count > 0)
            {
                AddSectionHeading(document, "Education", headingFont);
                foreach (var edu in cv.Education)
                {
                    var line = new Paragraph { SpacingBefore = 4, SpacingAfter = 1 };
                    line.Add(new Chunk(edu.Degree, boldBodyFont));
                    document.Add(line);
                    var meta = string.Join(" • ", new[] { edu.DateRange, edu.Institution }.Where(s => !string.IsNullOrWhiteSpace(s)));
                    if (meta.Length > 0)
                        document.Add(new Paragraph(meta, mutedBodyFont) { SpacingAfter = 2 });
                    foreach (var detail in edu.Details)
                        document.Add(new Paragraph($"•  {detail}", bodyFont) { SpacingAfter = 1, Leading = 13, IndentationLeft = 10 });
                }
                document.Add(new Paragraph(" ", bodyFont) { SpacingAfter = 4 });
            }

            if (cv.Certifications.Count > 0)
            {
                AddSectionHeading(document, "Certifications", headingFont);
                foreach (var cert in cv.Certifications)
                    document.Add(new Paragraph($"•  {cert}", bodyFont) { SpacingAfter = 2, Leading = 13 });
            }

            if (cv.Skills.AllSkills().Any())
            {
                AddSectionHeading(document, "Skills", headingFont);
                foreach (var (category, skills) in cv.Skills.AllCategories())
                {
                    if (skills.Count == 0) continue;
                    var line = new Paragraph { SpacingAfter = 3, Leading = 13 };
                    line.Add(new Chunk($"{category}: ", boldBodyFont));
                    line.Add(new Chunk(string.Join(", ", skills), bodyFont));
                    document.Add(line);
                }
            }

            if (cv.Projects.Count > 0)
            {
                AddSectionHeading(document, "Projects", headingFont);
                foreach (var project in cv.Projects)
                {
                    document.Add(new Paragraph(project.Name, boldBodyFont) { SpacingBefore = 4, SpacingAfter = 1 });
                    if (!string.IsNullOrWhiteSpace(project.Description))
                        document.Add(new Paragraph(project.Description, bodyFont) { SpacingAfter = 2, Leading = 13 });
                    if (project.Technologies.Count > 0)
                        document.Add(new Paragraph($"Tools: {string.Join(", ", project.Technologies)}", mutedBodyFont) { SpacingAfter = 3 });
                }
            }

            if (cv.Languages.Count > 0)
            {
                AddSectionHeading(document, "Languages", headingFont);
                document.Add(new Paragraph(string.Join("   •   ", cv.Languages), bodyFont) { SpacingAfter = 4 });
            }

            document.Close();
            return memoryStream.ToArray();
        }

        private void AddSectionHeading(Document document, string text, Font font)
        {
            var heading = new Paragraph(text.ToUpperInvariant(), font) { SpacingBefore = 8, SpacingAfter = 2 };
            document.Add(heading);
            AddRule(document);
        }

        private void AddRule(Document document)
        {
            var line = new Paragraph(" ") { SpacingAfter = 4 };
            var lineSeparator = new iTextSharp.text.pdf.draw.LineSeparator(0.75f, 100f, RuleColor, Element.ALIGN_LEFT, -2);
            line.Add(new Chunk(lineSeparator));
            document.Add(line);
        }
    }
}
