using Microsoft.AspNetCore.Mvc;
using CVBuilder.API.Models;
using CVBuilder.API.Models.DTOs;
using CVBuilder.API.Services;

namespace CVBuilder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CVController : ControllerBase
    {
        private readonly ICVGenerationService _cvGenerationService;
        private readonly ICVEnhancementService _cvEnhancementService;
        private readonly ILogger<CVController> _logger;

        public CVController(ICVGenerationService cvGenerationService, ICVEnhancementService cvEnhancementService, ILogger<CVController> logger)
        {
            _cvGenerationService = cvGenerationService;
            _cvEnhancementService = cvEnhancementService;
            _logger = logger;
        }

        [HttpPost("generate")]
        public async Task<ActionResult<CVGenerationResponse>> GenerateTailoredCV([FromBody] CVGenerationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.JobDescription))
                {
                    return BadRequest("Job description is required");
                }

                var result = await _cvGenerationService.GenerateTailoredCVAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GenerateTailoredCV: {ex.Message}");
                return StatusCode(500, "An error occurred while generating the tailored CV");
            }
        }

        [HttpPost("analyze")]
        public ActionResult<object> AnalyzeJobDescription([FromBody] string jobDescription)
        {
            try
            {
                if (string.IsNullOrEmpty(jobDescription))
                {
                    return BadRequest("Job description is required");
                }

                var skills = _cvGenerationService.ExtractKeywordsFromJobDescription(jobDescription);
                
                return Ok(new
                {
                    extractedSkills = skills,
                    skillCount = skills.Count,
                    analysisDate = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in AnalyzeJobDescription: {ex.Message}");
                return StatusCode(500, "An error occurred while analyzing the job description");
            }
        }

        [HttpGet("templates")]
        public ActionResult<object> GetAvailableTemplates()
        {
            var templates = new[]
            {
                new { id = "modern", name = "Modern", description = "Clean and contemporary design" },
                new { id = "classic", name = "Classic", description = "Traditional professional layout" },
                new { id = "minimalist", name = "Minimalist", description = "Minimal design with focus on content" },
                new { id = "creative", name = "Creative", description = "Modern design with color accents" }
            };

            return Ok(templates);
        }

        [HttpPost("enhance-from-pdf")]
        public async Task<ActionResult<EnhancedCVResponse>> EnhanceCVFromPDF([FromForm] CVUploadRequest request)
        {
            try
            {
                if (request.CVFile == null || request.CVFile.Length == 0)
                {
                    return BadRequest("Please upload a CV file");
                }

                if (string.IsNullOrEmpty(request.JobDescription))
                {
                    return BadRequest("Please provide a job description");
                }

                using (var memoryStream = new MemoryStream())
                {
                    await request.CVFile.CopyToAsync(memoryStream);
                    var pdfContent = memoryStream.ToArray();

                    var result = await _cvEnhancementService.EnhanceCVFromPDFAsync(pdfContent, request.JobDescription);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in EnhanceCVFromPDF: {ex.Message}");
                return StatusCode(500, "An error occurred while processing the PDF");
            }
        }

        [HttpPost("enhance-from-text")]
        public async Task<ActionResult<EnhancedCVResponse>> EnhanceCVFromText([FromBody] CVTextRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.CVText))
                {
                    return BadRequest("Please provide your CV text");
                }

                if (string.IsNullOrEmpty(request.JobDescription))
                {
                    return BadRequest("Please provide a job description");
                }

                var result = await _cvEnhancementService.EnhanceCVFromTextAsync(request.CVText, request.JobDescription);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in EnhanceCVFromText: {ex.Message}");
                return StatusCode(500, "An error occurred while enhancing the CV");
            }
        }

        [HttpPost("match-score")]
        public ActionResult<double> CalculateMatchScore([FromBody] object cvJobPair)
        {
            try
            {
                // Simplified implementation - in production, deserialize properly
                return Ok(new { matchScore = 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in CalculateMatchScore: {ex.Message}");
                return StatusCode(500, "An error occurred while calculating the match score");
            }
        }
    }
}
