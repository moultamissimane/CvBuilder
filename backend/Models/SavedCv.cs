namespace CVBuilder.API.Models
{
    /// <summary>
    /// A saved, job-tailored CV version. The original uploaded CV is never stored as a "SavedCv" row and
    /// is never overwritten by generating a new version — each tailoring run is its own separate record.
    /// </summary>
    public class SavedCv
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Title { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public string Company { get; set; } = "";
        public string TemplateId { get; set; } = "modern";
        public double MatchScore { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>The tailored StructuredCv, serialized as JSON.</summary>
        public string CvDataJson { get; set; } = "";
    }
}
