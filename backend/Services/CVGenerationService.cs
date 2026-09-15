using CVBuilder.API.Models;
using CVBuilder.API.Models.DTOs;
using System.Text.RegularExpressions;

namespace CVBuilder.API.Services
{
    public interface ICVGenerationService
    {
        Task<CVGenerationResponse> GenerateTailoredCVAsync(CVGenerationRequest request);
        double CalculateMatchScore(CV cv, JobDescription jobDescription);
        List<string> ExtractKeywordsFromJobDescription(string jobDescription);
        List<string> RecommendSkillsToAdd(CV cv, JobDescription jobDescription);
    }

    public class CVGenerationService : ICVGenerationService
    {
        private readonly ILogger<CVGenerationService> _logger;

        public CVGenerationService(ILogger<CVGenerationService> logger)
        {
            _logger = logger;
        }

        public Task<CVGenerationResponse> GenerateTailoredCVAsync(CVGenerationRequest request)
        {
            try
            {
                // Extract keywords from job description
                var keywords = ExtractKeywordsFromJobDescription(request.JobDescription);
                
                // Parse user CV (simplified - assumes JSON format)
                var userCV = System.Text.Json.JsonSerializer.Deserialize<CV>(request.UserCV) 
                    ?? new CV();

                // Calculate match score
                var matchScore = CalculateMatchScore(userCV, new JobDescription 
                { 
                    Content = request.JobDescription,
                    RequiredSkills = keywords
                });

                // Extract key sections from job description
                var responsibilities = ExtractResponsibilities(request.JobDescription);
                var qualifications = ExtractQualifications(request.JobDescription);
                var requiredSkills = ExtractSkills(request.JobDescription);

                // Generate tailored summary
                var tailoredSummary = GenerateTailoredSummary(userCV, requiredSkills, responsibilities);

                // Get recommendations
                var recommendations = GenerateRecommendations(userCV, requiredSkills, matchScore);

                var response = new CVGenerationResponse
                {
                    TailoredSummary = tailoredSummary,
                    KeySkillsToHighlight = requiredSkills.Take(8).ToList(),
                    RelevantExperiences = ExtractRelevantExperiences(userCV, requiredSkills),
                    MatchScore = matchScore,
                    Recommendations = recommendations
                };
                return Task.FromResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error generating tailored CV: {ex.Message}");
                throw;
            }
        }

        public double CalculateMatchScore(CV cv, JobDescription jobDescription)
        {
            double score = 0;
            double maxScore = 100;

            // 1. Skill matching (40% weight)
            var skillMatches = cv.Skills.Intersect(jobDescription.RequiredSkills, StringComparer.OrdinalIgnoreCase).Count();
            var skillScore = (skillMatches / (double)jobDescription.RequiredSkills.Count) * 40;
            score += skillScore;

            // 2. Experience relevance (30% weight)
            var relevantExperience = cv.Experiences
                .Where(e => !e.CurrentlyWorking ? 
                    (DateTime.Now - e.EndDate.GetValueOrDefault()).TotalDays / 365.25 < 5 : 
                    true)
                .Count();
            var expScore = (relevantExperience / Math.Max(cv.Experiences.Count, 1)) * 30;
            score += expScore;

            // 3. Education level (20% weight)
            var educationScore = cv.Education.Count > 0 ? 20 : 10;
            score += educationScore;

            // 4. Summary completeness (10% weight)
            var completenessScore = string.IsNullOrEmpty(cv.Summary) ? 0 : 10;
            score += completenessScore;

            return Math.Min(score, maxScore);
        }

        public List<string> ExtractKeywordsFromJobDescription(string jobDescription)
        {
            var keywords = new List<string>();
            
            // Common skill keywords to search for. The capture group stops at '.', ';', ',' AND line breaks
            // so a skill list that wraps onto the next line doesn't get glued into one giant fragment.
            var skillPatterns = new[]
            {
                @"(?:expertise in|skilled in|proficient in|experience with|knowledge of|familiar with)\s+([^.;,\r\n]+)",
                @"(?:required|must have|need)\s+(?:experience|knowledge|skills?)\s+(?:in|with|of)\s+([^.;,\r\n]+)"
            };

            foreach (var pattern in skillPatterns)
            {
                var matches = Regex.Matches(jobDescription, pattern, RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        // Strip parentheses before splitting so "AWS (S3, EC2)" yields "AWS", "S3", "EC2"
                        // instead of the comma inside the parens breaking it into "AWS (S3" + "EC2)".
                        var skillsText = match.Groups[1].Value.Replace("(", " ").Replace(")", " ");
                        var skills = skillsText
                            .Split(new[] { ",", " and ", "&", "/" }, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var skill in skills)
                        {
                            var cleaned = skill.Trim().Trim('.', ')', '(');
                            if (cleaned.Length > 2)
                            {
                                keywords.Add(cleaned);
                            }
                        }
                    }
                }
            }

            // Remove duplicates and return top keywords
            return keywords
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(20)
                .ToList();
        }

        private List<string> ExtractSkills(string jobDescription)
        {
            return ExtractKeywordsFromJobDescription(jobDescription);
        }

        private List<string> ExtractResponsibilities(string jobDescription)
        {
            var responsibilities = new List<string>();
            var lines = jobDescription.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            
            var inResponsibilities = false;
            foreach (var line in lines)
            {
                if (line.Contains("Responsibilities", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("responsibilities", StringComparison.OrdinalIgnoreCase))
                {
                    inResponsibilities = true;
                    continue;
                }
                
                if (inResponsibilities && line.StartsWith("-") || line.StartsWith("•"))
                {
                    responsibilities.Add(line.TrimStart('-', '•').Trim());
                }
            }

            return responsibilities.Take(5).ToList();
        }

        private List<string> ExtractQualifications(string jobDescription)
        {
            var qualifications = new List<string>();
            var lines = jobDescription.Split(new[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            
            var inQualifications = false;
            foreach (var line in lines)
            {
                if (line.Contains("Qualifications", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("Requirements", StringComparison.OrdinalIgnoreCase))
                {
                    inQualifications = true;
                    continue;
                }
                
                if (inQualifications && (line.StartsWith("-") || line.StartsWith("•")))
                {
                    qualifications.Add(line.TrimStart('-', '•').Trim());
                }
            }

            return qualifications.Take(5).ToList();
        }

        private string GenerateTailoredSummary(CV cv, List<string> requiredSkills, List<string> responsibilities)
        {
            if (string.IsNullOrEmpty(cv.Summary))
            {
                return $"Experienced professional with expertise in {string.Join(", ", requiredSkills.Take(3))}. " +
                       $"Proven track record in {string.Join(", ", responsibilities.Take(2).Select(r => r.ToLower()))}. " +
                       "Seeking to contribute skills and experience to drive business success.";
            }

            // Enhance existing summary with job-specific keywords
            var enhanced = cv.Summary;
            var relevantSkills = requiredSkills
                .Where(s => enhanced.Contains(s, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (relevantSkills.Count == 0)
            {
                enhanced += $" Proficient in {string.Join(", ", requiredSkills.Take(3))}.";
            }

            return enhanced;
        }

        private List<string> ExtractRelevantExperiences(CV cv, List<string> requiredSkills)
        {
            var relevant = new List<string>();

            foreach (var exp in cv.Experiences)
            {
                var score = 0;
                foreach (var skill in requiredSkills)
                {
                    if (exp.Description.Contains(skill, StringComparison.OrdinalIgnoreCase) ||
                        exp.JobTitle.Contains(skill, StringComparison.OrdinalIgnoreCase))
                    {
                        score++;
                    }
                }

                if (score > 0)
                {
                    relevant.Add($"{exp.JobTitle} at {exp.CompanyName} ({score} skill matches)");
                }
            }

            return relevant.OrderByDescending(r => 
                int.Parse(r.Split('(')[1].Split(' ')[0])).Take(5).ToList();
        }

        private List<string> GenerateRecommendations(CV cv, List<string> requiredSkills, double matchScore)
        {
            var recommendations = new List<string>();

            if (matchScore < 50)
            {
                recommendations.Add("Consider adding more relevant skills that match the job requirements.");
                recommendations.Add("Highlight transferable skills from your experience section.");
            }

            var missingSkills = requiredSkills
                .Where(s => !cv.Skills.Any(cs => cs.Contains(s, StringComparison.OrdinalIgnoreCase)))
                .Take(3)
                .ToList();

            if (missingSkills.Count > 0)
            {
                recommendations.Add($"Consider gaining experience in: {string.Join(", ", missingSkills)}");
            }

            if (cv.Experiences.Count < 2)
            {
                recommendations.Add("Add more work experience to strengthen your profile.");
            }

            if (string.IsNullOrEmpty(cv.Summary))
            {
                recommendations.Add("Add a professional summary that highlights your key strengths.");
            }

            if (cv.Education.Count == 0)
            {
                recommendations.Add("Add your education details to make your CV more complete.");
            }

            return recommendations.Take(3).ToList();
        }

        public List<string> RecommendSkillsToAdd(CV cv, JobDescription jobDescription)
        {
            var missing = jobDescription.RequiredSkills
                .Where(skill => !cv.Skills.Any(s => s.Contains(skill, StringComparison.OrdinalIgnoreCase)))
                .Take(5)
                .ToList();

            return missing;
        }
    }
}
