using CVBuilder.API.Models;

namespace CVBuilder.API.Services
{
    public interface ICvTailoringService
    {
        StructuredCv GenerateTailoredCv(StructuredCv original, StructuredJobDescription job, MatchAnalysis analysis);
    }

    /// <summary>
    /// Builds a tailored copy of a StructuredCv for a specific job. Everything here is a copy, a reorder,
    /// or a templated sentence built strictly from already-verified facts — there is no free-text rewrite
    /// step, so this class cannot fabricate a company, title, date, skill or achievement by construction.
    /// </summary>
    public class CvTailoringService : ICvTailoringService
    {
        public StructuredCv GenerateTailoredCv(StructuredCv original, StructuredJobDescription job, MatchAnalysis analysis)
        {
            var verifiedSkills = analysis.SkillMatches
                .Where(m => m.Status == SkillMatchStatus.Verified)
                .Select(m => m.Skill)
                .ToList();

            return new StructuredCv
            {
                // Personal info is never rewritten — copied exactly as the user wrote it.
                PersonalInfo = new PersonalInfo
                {
                    FullName = original.PersonalInfo.FullName,
                    Title = original.PersonalInfo.Title,
                    Email = original.PersonalInfo.Email,
                    Phone = original.PersonalInfo.Phone,
                    ContactLine = original.PersonalInfo.ContactLine
                },
                Summary = TailorSummary(original.Summary, job, verifiedSkills),
                Experience = original.Experience.Select(e => TailorExperience(e, verifiedSkills)).ToList(),
                RawExperienceText = original.RawExperienceText,
                Education = original.Education,
                Skills = TailorSkills(original.Skills, verifiedSkills),
                Certifications = original.Certifications,
                Projects = original.Projects,
                Languages = original.Languages
            };
        }

        /// <summary>
        /// Keeps the user's own summary verbatim and, only when justified by an actual verified match,
        /// appends one factual sentence naming the job title and the skills that already back it up.
        /// Never rewrites or removes anything from the original text.
        /// </summary>
        private string TailorSummary(string originalSummary, StructuredJobDescription job, List<string> verifiedSkills)
        {
            if (string.IsNullOrWhiteSpace(originalSummary) || verifiedSkills.Count == 0 || string.IsNullOrWhiteSpace(job.JobTitle))
                return originalSummary;

            var highlight = string.Join(", ", verifiedSkills.Take(4));
            var suffix = $" This background in {highlight} directly aligns with the {job.JobTitle} role.";

            // Don't restate skills the summary already mentions in its own words.
            if (originalSummary.Contains(job.JobTitle, StringComparison.OrdinalIgnoreCase))
                return originalSummary;

            return originalSummary.TrimEnd() + suffix;
        }

        /// <summary>
        /// Reorders (never rewrites or removes) the bullets and technology tags of one experience entry
        /// so the ones matching the target job surface first, and records a relevance score for the UI.
        /// </summary>
        private StructuredExperience TailorExperience(StructuredExperience original, List<string> verifiedSkills)
        {
            bool MentionsAnyVerifiedSkill(string text) =>
                verifiedSkills.Any(skill => TechKeywords.ContainsWholeWord(text, skill));

            var orderedDescription = original.Description
                .Select((bullet, index) => (bullet, index, relevant: MentionsAnyVerifiedSkill(bullet)))
                .OrderByDescending(x => x.relevant)
                .ThenBy(x => x.index) // stable within each relevance tier
                .Select(x => x.bullet)
                .ToList();

            var orderedTech = original.Technologies
                .Select((tech, index) => (tech, index, relevant: verifiedSkills.Any(s => IsSameSkill(s, tech))))
                .OrderByDescending(x => x.relevant)
                .ThenBy(x => x.index)
                .Select(x => x.tech)
                .ToList();

            var relevanceScore = original.Technologies.Count(t => verifiedSkills.Any(s => IsSameSkill(s, t)))
                + original.Description.Count(MentionsAnyVerifiedSkill);

            return new StructuredExperience
            {
                JobTitle = original.JobTitle,
                Company = original.Company,
                DateRange = original.DateRange,
                Description = orderedDescription,
                Technologies = orderedTech,
                RelevanceScore = relevanceScore
            };
        }

        /// <summary>
        /// Reorders each skill category so items matching the target job come first. Never adds a skill
        /// that wasn't already in the original list — missing/not-verified skills are analysis-only and
        /// never appear inside the generated CV itself.
        /// </summary>
        private SkillGroups TailorSkills(SkillGroups original, List<string> verifiedSkills)
        {
            List<string> Reorder(List<string> skills) =>
                skills
                    .Select((s, i) => (s, i, relevant: verifiedSkills.Any(v => IsSameSkill(v, s))))
                    .OrderByDescending(x => x.relevant)
                    .ThenBy(x => x.i)
                    .Select(x => x.s)
                    .ToList();

            return new SkillGroups
            {
                Frontend = Reorder(original.Frontend),
                Backend = Reorder(original.Backend),
                Databases = Reorder(original.Databases),
                Cloud = Reorder(original.Cloud),
                DevOps = Reorder(original.DevOps),
                Tools = Reorder(original.Tools),
                Other = Reorder(original.Other)
            };
        }

        /// <summary>
        /// True if two skill spellings refer to the same technology — e.g. "React" (the canonical job
        /// requirement) and "React.js" (how the user happened to write it on their own CV). Plain string
        /// equality would miss this and silently fail to prioritize a skill the user actually has.
        /// </summary>
        private static bool IsSameSkill(string a, string b) =>
            string.Equals(TechKeywords.Canonicalize(a), TechKeywords.Canonicalize(b), StringComparison.OrdinalIgnoreCase);
    }
}
