using CVBuilder.API.Models;

namespace CVBuilder.API.Services
{
    public class ValidationReport
    {
        public bool IsValid => Issues.Count == 0;
        public List<string> Issues { get; set; } = new();
    }

    public interface ICvValidationService
    {
        /// <summary>
        /// Diffs a tailored CV against the original it was built from. Because the tailoring engine only
        /// ever copies, reorders, or appends a templated sentence, this should always come back clean —
        /// it exists as an automated safety net (Step 7), not because fabrication is expected.
        /// </summary>
        ValidationReport Validate(StructuredCv original, StructuredCv tailored);
    }

    public class CvValidationService : ICvValidationService
    {
        public ValidationReport Validate(StructuredCv original, StructuredCv tailored)
        {
            var issues = new List<string>();

            // Every experience entry must correspond exactly to one the user actually has —
            // same company, same title, same dates. Nothing invented, nothing altered.
            foreach (var exp in tailored.Experience)
            {
                var existsInOriginal = original.Experience.Any(o =>
                    string.Equals(o.Company, exp.Company, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(o.JobTitle, exp.JobTitle, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(o.DateRange, exp.DateRange, StringComparison.OrdinalIgnoreCase));

                if (!existsInOriginal)
                {
                    issues.Add($"Experience entry \"{exp.JobTitle} at {exp.Company}\" does not match the original CV.");
                }

                // Bullets and tech tags may only be reordered — never introduced.
                var extraBullets = exp.Description.Except(
                    original.Experience.FirstOrDefault(o => string.Equals(o.Company, exp.Company, StringComparison.OrdinalIgnoreCase))
                        ?.Description ?? new List<string>());
                if (extraBullets.Any())
                {
                    issues.Add($"Experience at {exp.Company} contains bullet(s) not present in the original CV.");
                }
            }

            if (tailored.Experience.Count != original.Experience.Count)
            {
                issues.Add("The number of experience entries changed between the original and tailored CV.");
            }

            // Every skill listed in the tailored CV must already have been listed in the original.
            var originalSkills = new HashSet<string>(original.Skills.AllSkills(), StringComparer.OrdinalIgnoreCase);
            foreach (var skill in tailored.Skills.AllSkills())
            {
                if (!originalSkills.Contains(skill))
                {
                    issues.Add($"Skill \"{skill}\" appears in the tailored CV but was not in the original.");
                }
            }

            // Certifications, education and projects must never be invented.
            var originalCerts = new HashSet<string>(original.Certifications, StringComparer.OrdinalIgnoreCase);
            foreach (var cert in tailored.Certifications)
            {
                if (!originalCerts.Contains(cert))
                    issues.Add($"Certification \"{cert}\" was not in the original CV.");
            }

            var originalDegrees = new HashSet<string>(original.Education.Select(e => e.Degree + "|" + e.Institution), StringComparer.OrdinalIgnoreCase);
            foreach (var edu in tailored.Education)
            {
                if (!originalDegrees.Contains(edu.Degree + "|" + edu.Institution))
                    issues.Add($"Education entry \"{edu.Degree}\" was not in the original CV.");
            }

            var originalProjects = new HashSet<string>(original.Projects.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);
            foreach (var project in tailored.Projects)
            {
                if (!originalProjects.Contains(project.Name))
                    issues.Add($"Project \"{project.Name}\" was not in the original CV.");
            }

            // The summary may only be extended, never rewritten or shortened.
            if (!string.IsNullOrEmpty(original.Summary) && !tailored.Summary.Contains(original.Summary.TrimEnd()))
            {
                issues.Add("The professional summary was altered beyond appending a sentence.");
            }

            return new ValidationReport { Issues = issues };
        }
    }
}
