namespace CVBuilder.API.Models
{
    public class CV
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int? JobDescriptionId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // CV Sections
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        
        public List<Experience> Experiences { get; set; } = new();
        public List<Education> Education { get; set; } = new();
        public List<string> Skills { get; set; } = new();
        public List<string> Certifications { get; set; } = new();
        
        // Template preference
        public string TemplateId { get; set; } = "modern"; // modern, classic, minimalist
        
        // Matching score with job description
        public double MatchScore { get; set; } = 0;
    }

    public class Experience
    {
        public string CompanyName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CurrentlyWorking { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> Achievements { get; set; } = new();
    }

    public class Education
    {
        public string Institution { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string FieldOfStudy { get; set; } = string.Empty;
        public int GraduationYear { get; set; }
        public string Grade { get; set; } = string.Empty;
    }
}
