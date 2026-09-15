using CVBuilder.API.Models;

namespace CVBuilder.API.Services
{
    public interface ISkillMatchingService
    {
        MatchAnalysis Match(StructuredCv cv, StructuredJobDescription job);
    }

    /// <summary>
    /// Classifies each job requirement as Verified (the CV demonstrates it), Related (the job asked for a
    /// general concept and the CV has concrete evidence of that concept), or NotVerified (no evidence at
    /// all). Related is deliberately never used to imply the user knows a *specific* named technology they
    /// haven't demonstrated — e.g. if the CV only shows React, "Angular" stays NotVerified, it never
    /// becomes "Related" just because both are frontend frameworks.
    /// </summary>
    public class SkillMatchingService : ISkillMatchingService
    {
        // Generic concept phrases the job description might use, mapped to a check for concrete evidence
        // of that concept in the CV. These are the ONLY things that can become "Related" — a concrete
        // named technology (Angular, .NET, Python, ...) never qualifies, it's Verified or NotVerified.
        private static readonly (string[] Phrases, Func<string, StructuredCv, bool> HasEvidence)[] ConceptChecks = new (string[], Func<string, StructuredCv, bool>)[]
        {
            (new[] { "rest api", "restful api", "api integration", "api development" },
                (text, cv) => TechKeywords.ContainsWholeWord(EvidenceText(cv), "API")),

            (new[] { "cloud", "cloud computing", "cloud experience", "cloud platform" },
                (text, cv) => new[] { "AWS", "Azure", "GCP", "Docker", "Kubernetes", "Firebase", "Vercel", "Netlify" }
                    .Any(k => cv.Skills.AllSkills().Any(s => TechKeywords.ContainsWholeWord(s, k)) || TechKeywords.ContainsWholeWord(EvidenceText(cv), k))),

            (new[] { "ci/cd", "continuous integration", "continuous deployment", "continuous delivery" },
                (text, cv) => new[] { "Jenkins", "GitHub Actions", "GitLab CI", "CI/CD" }
                    .Any(k => TechKeywords.ContainsWholeWord(EvidenceText(cv), k))),

            (new[] { "version control", "source control" },
                (text, cv) => TechKeywords.ContainsWholeWord(EvidenceText(cv), "Git")),

            (new[] { "agile", "scrum", "agile methodology" },
                (text, cv) => EvidenceText(cv).Contains("agile", StringComparison.OrdinalIgnoreCase)
                    || EvidenceText(cv).Contains("scrum", StringComparison.OrdinalIgnoreCase)),

            (new[] { "database design", "databases", "data modeling" },
                (text, cv) => cv.Skills.Databases.Count > 0),

            (new[] { "testing", "unit testing", "test-driven development", "tdd" },
                (text, cv) => EvidenceText(cv).Contains("test", StringComparison.OrdinalIgnoreCase)),

            (new[] { "frontend framework", "frontend frameworks", "front-end framework" },
                (text, cv) => cv.Skills.Frontend.Count > 0),

            (new[] { "backend architecture", "backend development", "server-side development" },
                (text, cv) => cv.Skills.Backend.Count > 0 || cv.Experience.Any(e => e.Technologies.Count > 0)),

            (new[] { "responsive design", "ui/ux", "ui design" },
                (text, cv) => cv.Skills.Frontend.Count > 0),

            (new[] { "microservices", "microservice architecture" },
                (text, cv) => EvidenceText(cv).Contains("microservice", StringComparison.OrdinalIgnoreCase)),
        };

        public MatchAnalysis Match(StructuredCv cv, StructuredJobDescription job)
        {
            var matches = new List<SkillMatch>();

            foreach (var skill in job.RequiredSkills)
                matches.Add(ClassifySkill(skill, cv, isPreferred: false));

            foreach (var skill in job.PreferredSkills)
                matches.Add(ClassifySkill(skill, cv, isPreferred: true));

            var required = matches.Where(m => !m.IsPreferred).ToList();
            var preferred = matches.Where(m => m.IsPreferred).ToList();

            double Weight(SkillMatch m) => m.Status switch
            {
                SkillMatchStatus.Verified => 1.0,
                SkillMatchStatus.Related => 0.5,
                _ => 0.0
            };

            var requiredScore = required.Count > 0 ? required.Sum(Weight) / required.Count * 70 : 0;
            var preferredScore = preferred.Count > 0 ? preferred.Sum(Weight) / preferred.Count * 20 : 0;
            var experienceScore = Math.Min(cv.Experience.Count * 3, 10);

            var totalScore = Math.Min(100, requiredScore + preferredScore + experienceScore);

            return new MatchAnalysis
            {
                SkillMatches = matches,
                MatchScore = Math.Round(totalScore, 1),
                RequiredSkillCount = required.Count,
                VerifiedCount = matches.Count(m => m.Status == SkillMatchStatus.Verified),
                RelatedCount = matches.Count(m => m.Status == SkillMatchStatus.Related),
                NotVerifiedCount = matches.Count(m => m.Status == SkillMatchStatus.NotVerified)
            };
        }

        private SkillMatch ClassifySkill(string skill, StructuredCv cv, bool isPreferred)
        {
            var evidence = EvidenceText(cv);

            if (TechKeywords.ContainsWholeWord(evidence, skill))
            {
                return new SkillMatch { Skill = skill, Status = SkillMatchStatus.Verified, IsPreferred = isPreferred };
            }

            foreach (var (phrases, hasEvidence) in ConceptChecks)
            {
                if (phrases.Any(p => string.Equals(p, skill, StringComparison.OrdinalIgnoreCase)) && hasEvidence(skill, cv))
                {
                    return new SkillMatch
                    {
                        Skill = skill,
                        Status = SkillMatchStatus.Related,
                        IsPreferred = isPreferred,
                        Evidence = "Demonstrated through related technologies in your CV"
                    };
                }
            }

            return new SkillMatch { Skill = skill, Status = SkillMatchStatus.NotVerified, IsPreferred = isPreferred };
        }

        private static string EvidenceText(StructuredCv cv)
        {
            var parts = new List<string> { cv.Summary };
            parts.AddRange(cv.Skills.AllSkills());
            parts.AddRange(cv.Experience.SelectMany(e => e.Technologies));
            parts.AddRange(cv.Experience.SelectMany(e => e.Description));
            parts.AddRange(cv.Projects.SelectMany(p => p.Technologies));
            parts.Add(string.Join(" ", cv.Projects.Select(p => p.Description)));
            parts.AddRange(cv.Certifications);
            return string.Join(" \n ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }
    }
}
