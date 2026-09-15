namespace CVBuilder.API.Models
{
    public class JobDescription
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string UserId { get; set; } = string.Empty;

        // Extracted data
        public List<string> RequiredSkills { get; set; } = new();
        public List<string> KeyResponsibilities { get; set; } = new();
        public List<string> Qualifications { get; set; } = new();
    }
}
