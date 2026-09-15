using CVBuilder.API.Data;
using CVBuilder.API.Models;
using CVBuilder.API.Models.DTOs;
using CVBuilder.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CVBuilder.API.Controllers
{
    [ApiController]
    [Route("api/tailored-cv")]
    public class TailoredCvController : ControllerBase
    {
        private const int MaxTextLength = 200_000;
        private const long MaxUploadBytes = 10 * 1024 * 1024; // 10 MB

        private readonly ICVStructuringService _structuringService;
        private readonly ISkillMatchingService _matchingService;
        private readonly ICvTailoringService _tailoringService;
        private readonly ICvValidationService _validationService;
        private readonly IPdfExportService _pdfExportService;
        private readonly AppDbContext _db;
        private readonly ILogger<TailoredCvController> _logger;

        public TailoredCvController(
            ICVStructuringService structuringService,
            ISkillMatchingService matchingService,
            ICvTailoringService tailoringService,
            ICvValidationService validationService,
            IPdfExportService pdfExportService,
            AppDbContext db,
            ILogger<TailoredCvController> logger)
        {
            _structuringService = structuringService;
            _matchingService = matchingService;
            _tailoringService = tailoringService;
            _validationService = validationService;
            _pdfExportService = pdfExportService;
            _db = db;
            _logger = logger;
        }

        [HttpPost("analyze-text")]
        public ActionResult<AnalyzeResponse> AnalyzeText([FromBody] AnalyzeTextRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CvText))
                return BadRequest("Please provide your CV text.");
            if (string.IsNullOrWhiteSpace(request.JobDescription))
                return BadRequest("Please provide a job description.");
            if (request.CvText.Length > MaxTextLength || request.JobDescription.Length > MaxTextLength)
                return BadRequest("The CV or job description text is too long to process.");

            return Ok(BuildAnalysis(request.CvText, request.JobDescription));
        }

        [HttpPost("analyze-upload")]
        public async Task<ActionResult<AnalyzeResponse>> AnalyzeUpload([FromForm] AnalyzeUploadRequest request)
        {
            if (request.CvFile == null || request.CvFile.Length == 0)
                return BadRequest("Please upload a CV file.");
            if (request.CvFile.Length > MaxUploadBytes)
                return BadRequest("The uploaded file is too large (10 MB max).");
            if (!Path.GetExtension(request.CvFile.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only PDF files are supported.");
            if (string.IsNullOrWhiteSpace(request.JobDescription))
                return BadRequest("Please provide a job description.");

            try
            {
                using var memoryStream = new MemoryStream();
                await request.CvFile.CopyToAsync(memoryStream);
                var cvText = PdfTextReader.ExtractText(memoryStream.ToArray());

                if (string.IsNullOrWhiteSpace(cvText))
                    return BadRequest("Could not read any text from that PDF. It may be a scanned image rather than a text-based PDF.");

                return Ok(BuildAnalysis(cvText, request.JobDescription));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process uploaded CV PDF");
                return StatusCode(500, "That file couldn't be read as a PDF. Please check the file and try again.");
            }
        }

        [HttpPost("generate")]
        public ActionResult<GenerateTailoredCvResponse> GenerateTailoredCv([FromBody] GenerateTailoredCvRequest request)
        {
            if (request?.Cv == null || string.IsNullOrWhiteSpace(request.Cv.PersonalInfo?.FullName))
                return BadRequest("Missing or incomplete CV data — analyze a CV first.");
            if (request.Job == null)
                return BadRequest("Missing job description data — analyze a job description first.");

            try
            {
                var analysis = _matchingService.Match(request.Cv, request.Job);
                var tailored = _tailoringService.GenerateTailoredCv(request.Cv, request.Job, analysis);
                var validation = _validationService.Validate(request.Cv, tailored);

                if (!validation.IsValid)
                {
                    // Should never happen given how the tailoring engine is built — but if it does, log
                    // loudly rather than silently ship content that isn't backed by the source CV.
                    _logger.LogError("Tailored CV failed anti-fabrication validation: {Issues}", string.Join("; ", validation.Issues));
                }

                return Ok(new GenerateTailoredCvResponse
                {
                    TailoredCv = tailored,
                    Analysis = analysis,
                    ValidationIssues = validation.Issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate tailored CV");
                return StatusCode(500, "An error occurred while generating the tailored CV.");
            }
        }

        [HttpPost("export-pdf")]
        public ActionResult ExportPdf([FromBody] ExportPdfRequest request)
        {
            if (request?.Cv == null || string.IsNullOrWhiteSpace(request.Cv.PersonalInfo?.FullName))
                return BadRequest("Missing CV data to export.");

            try
            {
                var bytes = _pdfExportService.ExportToPdf(request.Cv);
                var fileName = SanitizeFileName(request.Cv.PersonalInfo.FullName) + ".pdf";
                return File(bytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF export failed");
                return StatusCode(500, "Could not generate the PDF. Please try again.");
            }
        }

        [HttpPost("save")]
        public async Task<ActionResult<SavedCvSummary>> SaveCv([FromBody] SaveCvRequest request)
        {
            if (request?.Cv == null || string.IsNullOrWhiteSpace(request.Cv.PersonalInfo?.FullName))
                return BadRequest("Missing CV data to save.");

            var entity = new SavedCv
            {
                Title = string.IsNullOrWhiteSpace(request.Title)
                    ? $"{request.Cv.PersonalInfo.FullName} - {request.JobTitle}".Trim(' ', '-')
                    : request.Title,
                JobTitle = request.JobTitle ?? "",
                Company = request.Company ?? "",
                MatchScore = request.MatchScore,
                CvDataJson = JsonSerializer.Serialize(request.Cv)
            };

            _db.SavedCvs.Add(entity);
            await _db.SaveChangesAsync();

            return Ok(new SavedCvSummary
            {
                Id = entity.Id,
                Title = entity.Title,
                JobTitle = entity.JobTitle,
                Company = entity.Company,
                MatchScore = entity.MatchScore,
                CreatedAt = entity.CreatedAt
            });
        }

        [HttpGet("saved")]
        public async Task<ActionResult<List<SavedCvSummary>>> ListSavedCvs()
        {
            var items = await _db.SavedCvs
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new SavedCvSummary
                {
                    Id = c.Id,
                    Title = c.Title,
                    JobTitle = c.JobTitle,
                    Company = c.Company,
                    MatchScore = c.MatchScore,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("saved/{id}")]
        public async Task<ActionResult<SavedCvDetail>> GetSavedCv(string id)
        {
            var entity = await _db.SavedCvs.FindAsync(id);
            if (entity == null)
                return NotFound();

            var cv = JsonSerializer.Deserialize<StructuredCv>(entity.CvDataJson) ?? new StructuredCv();

            return Ok(new SavedCvDetail
            {
                Id = entity.Id,
                Title = entity.Title,
                JobTitle = entity.JobTitle,
                Company = entity.Company,
                MatchScore = entity.MatchScore,
                CreatedAt = entity.CreatedAt,
                Cv = cv
            });
        }

        private AnalyzeResponse BuildAnalysis(string cvText, string jobDescription)
        {
            var cv = _structuringService.ParseCv(cvText);
            var job = _structuringService.ParseJobDescription(jobDescription);
            var analysis = _matchingService.Match(cv, job);

            return new AnalyzeResponse { Cv = cv, Job = job, Analysis = analysis };
        }

        private static string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var cleaned = new string(name.Where(c => !invalid.Contains(c)).ToArray()).Trim();
            return string.IsNullOrWhiteSpace(cleaned) ? "cv" : cleaned.Replace(" ", "-");
        }
    }
}
