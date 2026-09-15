using CVBuilder.API.Models;
using System.Text.RegularExpressions;
using static CVBuilder.API.Services.CvTextParsingUtils;

namespace CVBuilder.API.Services
{
    public interface ICVStructuringService
    {
        StructuredCv ParseCv(string cvText);
        StructuredJobDescription ParseJobDescription(string jobText);
    }

    /// <summary>
    /// Turns raw CV / job description text into the structured models the tailoring pipeline works with.
    /// Every field here is either copied verbatim from the source text or split from something that was
    /// literally present — nothing is inferred or invented.
    /// </summary>
    public class CVStructuringService : ICVStructuringService
    {
        private static readonly Regex DateRangeRegex = new(
            @"(\b(19|20)\d{2}\b|\b[A-Za-z]{3,9}\.?\s+\d{4}\b)\s*[-–—]\s*(Present|present|\b(19|20)\d{2}\b|\b[A-Za-z]{3,9}\.?\s+\d{4}\b)",
            RegexOptions.Compiled);

        private static readonly (string[] Labels, string Category)[] SkillCategoryLabels = new[]
        {
            (new[] { "CLOUD & DEVOPS", "CLOUD AND DEVOPS", "DEVOPS & CLOUD" }, "Cloud"),
            (new[] { "FRONTEND", "FRONT-END", "FRONT END" }, "Frontend"),
            (new[] { "BACKEND", "BACK-END", "BACK END" }, "Backend"),
            (new[] { "DATABASES", "DATABASE" }, "Databases"),
            (new[] { "CLOUD" }, "Cloud"),
            (new[] { "DEVOPS" }, "DevOps"),
            (new[] { "TOOLS" }, "Tools"),
        };

        public StructuredCv ParseCv(string cvText)
        {
            var (headerBlock, sections) = SplitIntoSections(cvText);
            var (fullName, title, contactLine) = ParseHeader(headerBlock, out var headerSummary);
            var email = ExtractEmail(contactLine.Length > 0 ? contactLine : headerBlock);
            var phone = ExtractPhone(contactLine.Length > 0 ? contactLine : headerBlock);

            var summary = sections.GetValueOrDefault("Summary", headerSummary).Trim();
            var experienceText = sections.GetValueOrDefault("Experience", "").Trim();
            var educationText = sections.GetValueOrDefault("Education", "").Trim();
            var certificationsText = sections.GetValueOrDefault("Certifications", "").Trim();
            var skillsText = sections.GetValueOrDefault("Skills", "").Trim();
            var languagesText = sections.GetValueOrDefault("Languages", "").Trim();
            var projectsText = sections.GetValueOrDefault("Projects", "").Trim();

            return new StructuredCv
            {
                PersonalInfo = new PersonalInfo
                {
                    FullName = fullName,
                    Title = title,
                    Email = email,
                    Phone = phone,
                    ContactLine = contactLine
                },
                Summary = summary,
                Experience = ParseExperienceEntries(experienceText),
                RawExperienceText = experienceText,
                Education = ParseEducationEntries(educationText),
                Certifications = ParseBulletList(certificationsText),
                Skills = ParseSkillGroups(skillsText),
                Projects = ParseProjects(projectsText),
                Languages = ParseLanguageList(languagesText)
            };
        }

        /// <summary>
        /// Splits an "Experience" section into individual job entries. An entry starts at a line shaped
        /// like "Job Title — Company" (an uppercase/digit-led line containing a dash separator). Everything
        /// until the next such line belongs to that entry: a date range if one appears, a "Tools:" /
        /// "Technologies:" line, and the remaining lines as description bullets.
        /// If no entry-start lines are found at all, an empty list is returned — callers should fall back
        /// to displaying RawExperienceText verbatim rather than guessing at structure.
        /// </summary>
        private List<StructuredExperience> ParseExperienceEntries(string experienceText)
        {
            var entries = new List<StructuredExperience>();
            if (string.IsNullOrWhiteSpace(experienceText))
                return entries;

            var lines = experienceText.Split('\n').Select(l => l.TrimEnd()).ToList();
            // Only the styled em/en dash counts as a title/company separator — a plain hyphen is too
            // common inside real titles ("Full-Stack Developer", "Front-End Engineer") to use safely.
            var entryStartRegex = new Regex(@"^(?<title>[A-Z0-9][^—–]{2,79}?)\s+[—–]\s+(?<company>[^—–].{1,59})$");

            var boundaries = new List<int>();
            for (int i = 0; i < lines.Count; i++)
            {
                var trimmed = lines[i].Trim();
                if (trimmed.Length == 0 || trimmed.Length > 90)
                    continue;

                // "Project: X – Y" lines use the same em/en dash and would otherwise look exactly like a
                // "Title — Company" entry header — exclude known non-title prefixes explicitly.
                if (Regex.IsMatch(trimmed, @"^(Project|Tools|Technologies|Tech Stack)\s*:", RegexOptions.IgnoreCase))
                    continue;

                if (entryStartRegex.IsMatch(trimmed))
                {
                    boundaries.Add(i);
                }
            }

            if (boundaries.Count == 0)
                return entries;

            for (int b = 0; b < boundaries.Count; b++)
            {
                var startLine = lines[boundaries[b]].Trim();
                var match = entryStartRegex.Match(startLine);
                var blockEnd = b + 1 < boundaries.Count ? boundaries[b + 1] : lines.Count;
                var blockLines = lines.Skip(boundaries[b] + 1).Take(blockEnd - boundaries[b] - 1)
                    .Select(l => l.Trim())
                    .Where(l => l.Length > 0)
                    .ToList();

                var entry = new StructuredExperience
                {
                    JobTitle = match.Groups["title"].Value.Trim(),
                    Company = match.Groups["company"].Value.Trim()
                };

                var description = new List<string>();
                foreach (var line in blockLines)
                {
                    if (entry.DateRange.Length == 0 && DateRangeRegex.IsMatch(line))
                    {
                        entry.DateRange = DateRangeRegex.Match(line).Value;
                        // A date line sometimes carries nothing else useful; skip adding it as a bullet.
                        var remainder = DateRangeRegex.Replace(line, "").Trim(' ', '•', '-', '|');
                        if (remainder.Length > 3)
                            description.Add(remainder);
                        continue;
                    }

                    if (Regex.IsMatch(line, @"^(Tools|Technologies|Tech Stack)\s*:", RegexOptions.IgnoreCase))
                    {
                        var techPart = Regex.Replace(line, @"^(Tools|Technologies|Tech Stack)\s*:", "", RegexOptions.IgnoreCase);
                        entry.Technologies = techPart.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                            .Where(t => t.Length > 0)
                            .ToList();
                        continue;
                    }

                    if (Regex.IsMatch(line, @"^(Missions?\s*&?\s*Achievements?|Key Achievements?)\s*:?\s*$", RegexOptions.IgnoreCase))
                        continue;

                    description.Add(line.TrimStart('•', '-', '*').Trim());
                }

                entry.Description = description;
                entries.Add(entry);
            }

            return entries;
        }

        private List<StructuredEducation> ParseEducationEntries(string educationText)
        {
            var entries = new List<StructuredEducation>();
            if (string.IsNullOrWhiteSpace(educationText))
                return entries;

            var lines = educationText.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();

            string pendingDegree = null;
            StructuredEducation current = null;

            foreach (var line in lines)
            {
                var isDateLine = DateRangeRegex.IsMatch(line);

                if (isDateLine)
                {
                    var degree = pendingDegree ?? current?.Degree ?? "";
                    var dateMatch = DateRangeRegex.Match(line).Value;
                    var rest = line.Replace(dateMatch, "").Trim(' ', '•', '-', '|');

                    current = new StructuredEducation
                    {
                        Degree = degree,
                        DateRange = dateMatch,
                        Institution = rest
                    };
                    entries.Add(current);
                    pendingDegree = null;
                    continue;
                }

                var looksLikeBullet = line.StartsWith("•") || line.StartsWith("-") || line.StartsWith("*");
                // A descriptive sentence ("Specialization in ... computation.") also starts with a capital
                // letter and is often under 100 chars, so it would otherwise be indistinguishable from a
                // real heading like "FULL STACK JAVASCRIPT DEVELOPMENT". Requiring no trailing period is
                // what actually separates the two here — headings don't end a CV in punctuation, sentences do.
                var looksLikeNewHeading = !looksLikeBullet && !line.TrimEnd().EndsWith(".") && line.Length < 100 &&
                    (char.IsUpper(line[0]) || char.IsDigit(line[0]));

                if (current == null)
                {
                    pendingDegree = pendingDegree == null ? line : pendingDegree + " " + line;
                    continue;
                }

                if (looksLikeBullet || !looksLikeNewHeading)
                {
                    current.Details.Add(line.TrimStart('•', '-', '*').Trim());
                }
                else
                {
                    // Looks like the start of the next degree entry without its own date line yet.
                    pendingDegree = line;
                    current = null;
                }
            }

            return entries;
        }

        private SkillGroups ParseSkillGroups(string skillsText)
        {
            var groups = new SkillGroups();
            if (string.IsNullOrWhiteSpace(skillsText))
                return groups;

            void AddTo(string category, IEnumerable<string> skills)
            {
                var bucket = category switch
                {
                    "Frontend" => groups.Frontend,
                    "Backend" => groups.Backend,
                    "Databases" => groups.Databases,
                    "Cloud" => groups.Cloud,
                    "DevOps" => groups.DevOps,
                    "Tools" => groups.Tools,
                    _ => groups.Other
                };
                bucket.AddRange(skills);
            }

            foreach (var rawLine in skillsText.Split('\n'))
            {
                var line = rawLine.Trim();
                if (line.Length == 0)
                    continue;

                string matchedCategory = null;
                string remainder = line;

                foreach (var (labels, category) in SkillCategoryLabels)
                {
                    foreach (var label in labels)
                    {
                        if (line.ToUpperInvariant().StartsWith(label))
                        {
                            matchedCategory = category;
                            remainder = line.Substring(label.Length).TrimStart(':', ' ');
                            break;
                        }
                    }
                    if (matchedCategory != null) break;
                }

                var tokens = remainder
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(t => t.TrimEnd('.'))
                    .Where(t => t.Length > 0 && t.Length < 40)
                    .ToList();

                if (tokens.Count == 0)
                    continue;

                AddTo(matchedCategory ?? "Other", tokens);
            }

            return groups;
        }

        private List<StructuredProject> ParseProjects(string projectsText)
        {
            var projects = new List<StructuredProject>();
            if (string.IsNullOrWhiteSpace(projectsText))
                return projects;

            var paragraphs = Regex.Split(projectsText.Trim(), @"\n\s*\n")
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();

            foreach (var paragraph in paragraphs)
            {
                var lines = paragraph.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0).ToList();
                if (lines.Count == 0)
                    continue;

                var techLine = lines.FirstOrDefault(l => Regex.IsMatch(l, @"^(Tools|Technologies|Tech Stack)\s*:", RegexOptions.IgnoreCase));
                var technologies = techLine != null
                    ? Regex.Replace(techLine, @"^(Tools|Technologies|Tech Stack)\s*:", "", RegexOptions.IgnoreCase)
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : new List<string>();

                var descriptionLines = lines.Skip(1).Where(l => l != techLine);

                projects.Add(new StructuredProject
                {
                    Name = lines[0].TrimStart('•', '-', '*').Trim(),
                    Description = string.Join(" ", descriptionLines),
                    Technologies = technologies
                });
            }

            return projects;
        }

        private List<string> ParseBulletList(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            return text.Split('\n')
                .Select(l => l.Trim().TrimStart('•', '-', '*').Trim())
                .Where(l => l.Length > 0)
                .ToList();
        }

        private List<string> ParseLanguageList(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            return text.Split(new[] { '|', '\n', '•' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();
        }

        public StructuredJobDescription ParseJobDescription(string jobText)
        {
            jobText ??= "";
            var (headerBlock, sections) = SplitJobSections(jobText);

            var jobTitle = ExtractJobTitle(headerBlock, jobText);
            var company = ExtractCompany(headerBlock);

            var requiredText = sections.GetValueOrDefault("Required", "");
            var preferredText = sections.GetValueOrDefault("Preferred", "");
            var responsibilitiesText = sections.GetValueOrDefault("Responsibilities", "");

            List<string> requiredSkills;
            List<string> preferredSkills;

            if (requiredText.Length == 0 && preferredText.Length == 0)
            {
                // No explicit requirements/preferred split found — treat the whole posting as the
                // requirements pool rather than guessing at a split that isn't there.
                requiredSkills = ExtractSkillMentions(jobText);
                preferredSkills = new List<string>();
            }
            else
            {
                requiredSkills = ExtractSkillMentions(requiredText.Length > 0 ? requiredText : jobText);
                preferredSkills = ExtractSkillMentions(preferredText)
                    .Where(s => !requiredSkills.Contains(s, StringComparer.OrdinalIgnoreCase))
                    .ToList();
            }

            var experienceMatch = Regex.Match(jobText, @"(\d+)\+?\s*(?:to\s*\d+\s*)?years?", RegexOptions.IgnoreCase);

            return new StructuredJobDescription
            {
                JobTitle = jobTitle,
                Company = company,
                RequiredSkills = requiredSkills,
                PreferredSkills = preferredSkills,
                Responsibilities = ParseBulletList(responsibilitiesText),
                Keywords = requiredSkills.Concat(preferredSkills).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                ExperienceRequirement = experienceMatch.Success ? experienceMatch.Value : "",
                RawText = jobText
            };
        }

        private static readonly (string Key, string[] Headings)[] JobSections = new[]
        {
            ("Required", new[] { "REQUIREMENTS", "REQUIRED SKILLS", "MUST HAVE", "QUALIFICATIONS", "MINIMUM QUALIFICATIONS", "WHAT YOU NEED", "WHAT WE'RE LOOKING FOR" }),
            ("Preferred", new[] { "NICE TO HAVE", "PREFERRED", "PREFERRED SKILLS", "BONUS", "PLUS", "GOOD TO HAVE", "NICE-TO-HAVES" }),
            ("Responsibilities", new[] { "RESPONSIBILITIES", "KEY RESPONSIBILITIES", "WHAT YOU'LL DO", "WHAT YOU WILL DO", "THE ROLE", "ROLE" }),
        };

        private (string HeaderBlock, Dictionary<string, string> Sections) SplitJobSections(string jobText)
        {
            var sections = new Dictionary<string, string>();
            var lines = jobText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var headerLines = new List<string>();
            string currentKey = null;
            var buffer = new List<string>();

            string MatchJobHeading(string rawLine)
            {
                var line = rawLine?.Trim().TrimEnd(':');
                if (string.IsNullOrWhiteSpace(line) || line.Length > 50)
                    return null;
                var normalized = line.ToUpperInvariant();
                foreach (var (key, headings) in JobSections)
                {
                    if (headings.Any(h => normalized == h))
                        return key;
                }
                return null;
            }

            void Flush()
            {
                if (currentKey == null) return;
                var text = string.Join("\n", buffer).Trim();
                sections[currentKey] = sections.TryGetValue(currentKey, out var existing) && existing.Length > 0
                    ? existing + "\n" + text
                    : text;
            }

            foreach (var rawLine in lines)
            {
                var matched = MatchJobHeading(rawLine);
                if (matched != null)
                {
                    if (currentKey == null) headerLines.AddRange(buffer);
                    else Flush();
                    currentKey = matched;
                    buffer.Clear();
                    continue;
                }
                buffer.Add(rawLine.TrimEnd());
            }

            if (currentKey == null) headerLines.AddRange(buffer);
            else Flush();

            return (string.Join("\n", headerLines).Trim(), sections);
        }

        private List<string> ExtractSkillMentions(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            return TechKeywords.ExtractKnownMentions(text)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private string ExtractJobTitle(string headerBlock, string fullText)
        {
            var labeled = Regex.Match(fullText, @"(?:Job Title|Position|Role)\s*:\s*(.+)", RegexOptions.IgnoreCase);
            if (labeled.Success)
                return labeled.Groups[1].Value.Trim();

            var firstLine = headerBlock.Split('\n').Select(l => l.Trim()).FirstOrDefault(l => l.Length > 0);
            if (firstLine != null && firstLine.Length < 80 && !firstLine.EndsWith("."))
                return firstLine;

            return "";
        }

        private string ExtractCompany(string headerBlock)
        {
            var labeled = Regex.Match(headerBlock, @"(?:Company|Employer)\s*:\s*(.+)", RegexOptions.IgnoreCase);
            return labeled.Success ? labeled.Groups[1].Value.Trim() : "";
        }
    }
}
