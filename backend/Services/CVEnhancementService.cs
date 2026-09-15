using CVBuilder.API.Models;
using CVBuilder.API.Models.DTOs;
using System.Text.RegularExpressions;
using static CVBuilder.API.Services.CvTextParsingUtils;

namespace CVBuilder.API.Services
{
    public interface ICVEnhancementService
    {
        Task<EnhancedCVResponse> EnhanceCVFromPDFAsync(byte[] pdfContent, string jobDescription);
        Task<EnhancedCVResponse> EnhanceCVFromTextAsync(string cvText, string jobDescription);
    }

    public class CVEnhancementService : ICVEnhancementService
    {
        private readonly ICVGenerationService _cvGenerationService;
        private readonly ILogger<CVEnhancementService> _logger;

        public CVEnhancementService(ICVGenerationService cvGenerationService, ILogger<CVEnhancementService> logger)
        {
            _cvGenerationService = cvGenerationService;
            _logger = logger;
        }

        public async Task<EnhancedCVResponse> EnhanceCVFromPDFAsync(byte[] pdfContent, string jobDescription)
        {
            try
            {
                var cvText = PdfTextReader.ExtractText(pdfContent);
                return await EnhanceCVFromTextAsync(cvText, jobDescription);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing PDF: {ex.Message}");
                throw;
            }
        }

        public Task<EnhancedCVResponse> EnhanceCVFromTextAsync(string cvText, string jobDescription)
        {
            try
            {
                // Extract job requirements
                var jobSkills = _cvGenerationService.ExtractKeywordsFromJobDescription(jobDescription);

                // Split the user's own CV into its existing sections, keeping each one's original wording intact
                var (headerBlock, sections) = SplitIntoSections(cvText);
                var (fullName, title, contactLine) = ParseHeader(headerBlock, out var headerSummary);

                var email = ExtractEmail(contactLine.Length > 0 ? contactLine : headerBlock);
                var phone = ExtractPhone(contactLine.Length > 0 ? contactLine : headerBlock);

                var summary = sections.GetValueOrDefault("Summary", headerSummary).Trim();
                var skillsText = sections.GetValueOrDefault("Skills", "").Trim();
                var experienceText = sections.GetValueOrDefault("Experience", "").Trim();
                var educationText = sections.GetValueOrDefault("Education", "").Trim();
                var certificationsText = sections.GetValueOrDefault("Certifications", "").Trim();
                var languagesText = sections.GetValueOrDefault("Languages", "").Trim();

                var existingSkills = ParseSkillTokens(skillsText);

                // Only what's genuinely missing from the user's own skills list gets proposed as an addition
                var missingSkills = jobSkills
                    .Where(skill => !existingSkills.Any(es => es.Contains(skill, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                var enhancedSummary = EnhanceSummary(summary, jobSkills);
                var addedSkills = missingSkills
                    .Select(s => new SkillAddition { Skill = s, Level = "Familiar with", IsAdded = true })
                    .ToList();
                var syntheticExperience = GenerateSyntheticExperience(missingSkills);

                var experienceCount = Math.Max(1, Regex.Matches(experienceText, @"\b(19|20)\d{2}\b").Count / 2);
                var matchScore = CalculateEnhancedMatchScore(existingSkills, jobSkills, experienceCount);

                var response = new EnhancedCVResponse
                {
                    OriginalCV = cvText,
                    FullName = fullName,
                    Title = title,
                    ContactLine = contactLine,
                    Email = email,
                    Phone = phone,
                    Summary = enhancedSummary,
                    ExperienceText = experienceText,
                    EducationText = educationText,
                    CertificationsText = certificationsText,
                    SkillsText = skillsText,
                    LanguagesText = languagesTgext,
                    AddedSkills = addedSkills,
                    SyntheticExperience = syntheticExperience,
                    MatchScore = matchScore,
                    Recommendations = GenerateRecommendations(missingSkills, syntheticExperience)
                };

                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error enhancing CV: {ex.Message}");
                throw;
            }
        }

        private string EnhanceSummary(string existingSummary, List<string> jobSkills)
        {
            if (string.IsNullOrWhiteSpace(existingSummary))
            {
                return $"Skilled professional with expertise in {string.Join(", ", jobSkills.Take(3))}. " +
                       "Experienced in developing solutions and driving business impact. " +
                       "Seeking to leverage technical expertise to contribute to innovative projects.";
            }

            var enhanced = existingSummary;
            var toMention = jobSkills
                .Where(skill => !enhanced.Contains(skill, StringComparison.OrdinalIgnoreCase))
                .Take(3)
                .ToList();

            if (toMention.Count > 0)
            {
                enhanced += $" Also proficient in {string.Join(", ", toMention)}.";
            }

            return enhanced;
        }

        private SyntheticExperienceAddition GenerateSyntheticExperience(List<string> missingSkills)
        {
            if (missingSkills.Count == 0)
                return null;

            var skillsToAdd = missingSkills.Take(3).ToList();

            return new SyntheticExperienceAddition
            {
                Title = "Relevant Project Experience (Suggested)",
                Description = $"Developed and maintained projects utilizing {string.Join(", ", skillsToAdd)}. " +
                              "Implemented best practices and optimized performance. " +
                              "Collaborated with cross-functional teams to deliver high-quality solutions.",
                SkillsUsed = skillsToAdd,
                IsProposed = true
            };
        }

        private double CalculateEnhancedMatchScore(List<string> userSkills, List<string> jobSkills, int experienceCount)
        {
            double score = 0;

            // Original skill matches (50% weight)
            var matches = userSkills.Count(us => jobSkills.Any(js => us.Contains(js, StringComparison.OrdinalIgnoreCase)));
            var skillScore = (matches / (double)Math.Max(jobSkills.Count, 1)) * 50;
            score += Math.Min(skillScore, 50);

            // Experience count (30% weight)
            var expScore = Math.Min(experienceCount * 6, 30);
            score += expScore;

            // After enhancement (20% weight) - shows potential with learning
            score += 20;

            return Math.Min(score, 100);
        }

        private List<string> GenerateRecommendations(List<string> missingSkills, SyntheticExperienceAddition syntheticExp)
        {
            var recommendations = new List<string>();

            if (missingSkills.Count > 0)
            {
                recommendations.Add($"🎓 Consider learning: {string.Join(", ", missingSkills.Take(2))}");
            }

            if (syntheticExp != null)
            {
                recommendations.Add("💡 A relevant project has been added to highlight your potential in required technologies.");
            }

            recommendations.Add("📌 Review the enhanced CV and customize the suggested project section with your own relevant projects.");
            recommendations.Add("✨ Focus on gaining hands-on experience with the highlighted technologies to strengthen your profile.");

            return recommendations;
        }
    }
}
