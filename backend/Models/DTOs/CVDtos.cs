namespace CVBuilder.API.Models.DTOs
{
    public class CreateCVRequest
    {
        public string Title { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<ExperienceDto> Experiences { get; set; } = new();
        public List<EducationDto> Education { get; set; } = new();
        public List<string> Skills { get; set; } = new();
    }

    public class UpdateCVRequest
    {
        public string? Title { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Summary { get; set; }
        public List<ExperienceDto>? Experiences { get; set; }
        public List<EducationDto>? Education { get; set; }
        public List<string>? Skills { get; set; }
        public string? TemplateId { get; set; }
    }

    public class CVGenerationRequest
    {
        public string JobDescription { get; set; } = string.Empty;
        public string UserCV { get; set; } = string.Empty; // Could be JSON string of existing CV
    }

    public class CVGenerationResponse
    {
        public string TailoredSummary { get; set; } = string.Empty;
        public List<string> KeySkillsToHighlight { get; set; } = new();
        public List<string> RelevantExperiences { get; set; } = new();
        public double MatchScore { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    public class ExperienceDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool CurrentlyWorking { get; set; }
        public string Description { get; set; } = string.Empty;
        public List<string> Achievements { get; set; } = new();
    }

    public class EducationDto
    {
        public string Institution { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string FieldOfStudy { get; set; } = string.Empty;
        public int GraduationYear { get; set; }
        public string Grade { get; set; } = string.Empty;
    }

    public class CVResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public List<ExperienceDto> Experiences { get; set; } = new();
        public List<EducationDto> Education { get; set; } = new();
        public List<string> Skills { get; set; } = new();
        public string TemplateId { get; set; } = "modern";
        public double MatchScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
