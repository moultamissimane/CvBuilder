namespace CVBuilder.API.Models
{
    /// <summary>
    /// A CV normalized into a structured shape. This is the source of truth for the Tailored CV
    /// Generator: everything the generator produces is copied or reordered from here — nothing is
    /// invented, so there is no free-text generation step that could fabricate content.
    /// </summary>
    public class StructuredCv
    {
        public PersonalInfo PersonalInfo { get; set; } = new();
        public string Summary { get; set; } = "";
        public List<StructuredExperience> Experience { get; set; } = new();
        public List<StructuredEducation> Education { get; set; } = new();
        public SkillGroups Skills { get; set; } = new();
        public List<string> Certifications { get; set; } = new();
        public List<StructuredProject> Projects { get; set; } = new();
        public List<string> Languages { get; set; } = new();

        /// <summary>
        /// Original, unparsed "Experience" section text. Kept as a safety-net fallback: if the entry
        /// parser can't confidently split this CV's format into individual jobs, the UI can still show
        /// this verbatim instead of silently dropping the section.
        /// </summary>
        public string RawExperienceText { get; set; } = "";
    }

    public class PersonalInfo
    {
        public string FullName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string ContactLine { get; set; } = "";
    }

    public class StructuredExperience
    {
        public string JobTitle { get; set; } = "";
        public string Company { get; set; } = "";
        public string DateRange { get; set; } = "";
        public List<string> Description { get; set; } = new();
        public List<string> Technologies { get; set; } = new();

        /// <summary>Computed against a specific job description; not persisted as part of the source CV.</summary>
        public int RelevanceScore { get; set; } = 0;
    }

    public class StructuredEducation
    {
        public string Degree { get; set; } = "";
        public string Institution { get; set; } = "";
        public string DateRange { get; set; } = "";
        public List<string> Details { get; set; } = new();
    }

    public class StructuredProject
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Technologies { get; set; } = new();
    }

    public class SkillGroups
    {
        public List<string> Frontend { get; set; } = new();
        public List<string> Backend { get; set; } = new();
        public List<string> Databases { get; set; } = new();
        public List<string> Cloud { get; set; } = new();
        public List<string> DevOps { get; set; } = new();
        public List<string> Tools { get; set; } = new();
        public List<string> Other { get; set; } = new();

        public IEnumerable<(string Category, List<string> Skills)> AllCategories()
        {
            yield return ("Frontend", Frontend);
            yield return ("Backend", Backend);
            yield return ("Databases", Databases);
            yield return ("Cloud", Cloud);
            yield return ("DevOps", DevOps);
            yield return ("Tools", Tools);
            yield return ("Other", Other);
        }

        public IEnumerable<string> AllSkills() => AllCategories().SelectMany(c => c.Skills);
    }

    public class StructuredJobDescription
    {
        public string JobTitle { get; set; } = "";
        public string Company { get; set; } = "";
        public List<string> RequiredSkills { get; set; } = new();
        public List<string> PreferredSkills { get; set; } = new();
        public List<string> Responsibilities { get; set; } = new();
        public List<string> Keywords { get; set; } = new();
        public string ExperienceRequirement { get; set; } = "";
        public string RawText { get; set; } = "";
    }
}
