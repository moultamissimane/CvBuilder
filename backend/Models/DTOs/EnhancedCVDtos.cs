namespace CVBuilder.API.Models.DTOs
{
    public class EnhancedCVResponse
    {
        public string OriginalCV { get; set; }

        // Parsed straight from the header of the user's own CV — kept as-is
        public string FullName { get; set; }
        public string Title { get; set; }
        public string ContactLine { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        // These are the user's original section blocks, preserved verbatim (same wording/order/template)
        public string Summary { get; set; }
        public string ExperienceText { get; set; }
        public string EducationText { get; set; }
        public string CertificationsText { get; set; }
        public string SkillsText { get; set; }
        public string LanguagesText { get; set; }

        // These are the AI additions, kept separate so they slot in at the end of their section instead of
        // being mixed into the user's original wording
        public List<SkillAddition> AddedSkills { get; set; }
        public SyntheticExperienceAddition SyntheticExperience { get; set; }

        public double MatchScore { get; set; }
        public List<string> Recommendations { get; set; }
    }

    public class SkillAddition
    {
        public string Skill { get; set; }
        public string Level { get; set; } // "Expert", "Familiar with", "Learning"
        public bool IsAdded { get; set; } = false;
    }

    public class SyntheticExperienceAddition
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> SkillsUsed { get; set; }
        public bool IsProposed { get; set; }
    }

    public class CVUploadRequest
    {
        public IFormFile CVFile { get; set; }
        public string JobDescription { get; set; }
    }

    public class CVTextRequest
    {
        public string CVText { get; set; }
        public string JobDescription { get; set; }
    }

    public class EnhancedCVDownloadRequest
    {
        public EnhancedCVResponse EnhancedCV { get; set; }
        public string FileName { get; set; }
    }
}
