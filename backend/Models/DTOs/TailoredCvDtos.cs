using CVBuilder.API.Models;

namespace CVBuilder.API.Models.DTOs
{
    public class AnalyzeTextRequest
    {
        public string CvText { get; set; } = "";
        public string JobDescription { get; set; } = "";
    }

    public class AnalyzeUploadRequest
    {
        public IFormFile CvFile { get; set; } = null!;
        public string JobDescription { get; set; } = "";
    }

    public class AnalyzeResponse
    {
        public StructuredCv Cv { get; set; } = new();
        public StructuredJobDescription Job { get; set; } = new();
        public MatchAnalysis Analysis { get; set; } = new();
    }

    public class GenerateTailoredCvRequest
    {
        public StructuredCv Cv { get; set; } = new();
        public StructuredJobDescription Job { get; set; } = new();
    }

    public class GenerateTailoredCvResponse
    {
        public StructuredCv TailoredCv { get; set; } = new();
        public MatchAnalysis Analysis { get; set; } = new();
        public List<string> ValidationIssues { get; set; } = new();
    }

    public class ExportPdfRequest
    {
        public StructuredCv Cv { get; set; } = new();
    }

    public class SaveCvRequest
    {
        public StructuredCv Cv { get; set; } = new();
        public string Title { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public string Company { get; set; } = "";
        public double MatchScore { get; set; }
    }

    public class SavedCvSummary
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public string Company { get; set; } = "";
        public double MatchScore { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SavedCvDetail
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public string Company { get; set; } = "";
        public double MatchScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public StructuredCv Cv { get; set; } = new();
    }
}
