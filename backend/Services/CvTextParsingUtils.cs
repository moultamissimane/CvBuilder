using System.Text.RegularExpressions;

namespace CVBuilder.API.Services
{
    /// <summary>
    /// Shared, dependency-free text parsing helpers for splitting a raw CV (or job posting) into its
    /// existing sections. Used by both the CV Enhancer flow and the Tailored CV Generator so the two
    /// don't maintain two copies of the same heuristics.
    /// </summary>
    public static class CvTextParsingUtils
    {
        // Section headings we recognize in a CV, keyed by canonical section name.
        // Matched against a full, trimmed, uppercased line so we don't grab headings out of running prose.
        public static readonly (string Key, string[] Headings)[] KnownSections = new[]
        {
            ("Summary", new[] { "SUMMARY", "PROFESSIONAL SUMMARY", "PROFILE", "OBJECTIVE", "ABOUT ME" }),
            ("Experience", new[] { "PROFESSIONAL EXPERIENCE", "WORK EXPERIENCE", "EXPERIENCE", "EMPLOYMENT HISTORY" }),
            ("Education", new[] { "EDUCATION", "ACADEMIC BACKGROUND" }),
            ("Certifications", new[] { "CERTIFICATIONS", "CERTIFICATES", "CERTIFICATIONS & TRAINING" }),
            ("Skills", new[] { "TECHNICAL SKILLS", "SKILLS", "CORE COMPETENCIES", "COMPETENCIES" }),
            ("Languages", new[] { "LANGUAGES", "LANGUES" }),
            ("Projects", new[] { "PROJECTS", "PERSONAL PROJECTS", "PROJETS" }),
        };

        /// <summary>
        /// Splits a CV's raw text into its existing sections (Experience, Education, Skills, ...) based on
        /// recognized section headings, plus everything before the first heading (name/title/contact/summary).
        /// Content inside each section is preserved verbatim, line breaks included.
        /// </summary>
        public static (string HeaderBlock, Dictionary<string, string> Sections) SplitIntoSections(string cvText)
        {
            var sections = new Dictionary<string, string>();
            var lines = (cvText ?? "").Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            var headerLines = new List<string>();
            string currentKey = null;
            var buffer = new List<string>();

            void FlushBuffer()
            {
                if (currentKey == null)
                    return;

                var text = string.Join("\n", buffer).Trim('\n');
                if (sections.TryGetValue(currentKey, out var existing) && !string.IsNullOrWhiteSpace(existing))
                {
                    sections[currentKey] = existing + "\n" + text;
                }
                else
                {
                    sections[currentKey] = text;
                }
            }

            foreach (var rawLine in lines)
            {
                var matchedKey = MatchHeading(rawLine);

                if (matchedKey != null)
                {
                    if (currentKey == null)
                    {
                        headerLines.AddRange(buffer);
                    }
                    else
                    {
                        FlushBuffer();
                    }

                    currentKey = matchedKey;
                    buffer.Clear();
                    continue;
                }

                buffer.Add(rawLine.TrimEnd());
            }

            if (currentKey == null)
            {
                headerLines.AddRange(buffer);
            }
            else
            {
                FlushBuffer();
            }

            return (string.Join("\n", headerLines).Trim('\n'), sections);
        }

        public static string MatchHeading(string rawLine)
        {
            var line = rawLine?.Trim().TrimEnd(':');
            if (string.IsNullOrWhiteSpace(line) || line.Length > 40)
                return null;

            var normalized = line.ToUpperInvariant();

            foreach (var (key, headings) in KnownSections)
            {
                if (headings.Any(h => normalized == h))
                    return key;
            }

            return null;
        }

        /// <summary>
        /// Pulls the name, title and contact line out of the block of text that appears before the CV's
        /// first recognized section heading, and returns whatever's left as a fallback summary paragraph
        /// (used when the CV has no explicit "Summary" section, but starts with a profile paragraph instead).
        /// </summary>
        public static (string FullName, string Title, string ContactLine) ParseHeader(string headerBlock, out string fallbackSummary)
        {
            var lines = (headerBlock ?? "")
                .Split('\n')
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .ToList();

            fallbackSummary = "";

            if (lines.Count == 0)
                return ("", "", "");

            var fullName = lines[0];

            var contactIndex = lines.FindIndex(1, lines.Count - 1, l =>
                l.Contains("@") || Regex.IsMatch(l, @"\+?\d[\d\s().-]{6,}\d"));

            if (contactIndex < 0)
            {
                fallbackSummary = string.Join(" ", lines.Skip(1));
                return (fullName, "", "");
            }

            var title = contactIndex > 1 ? string.Join(" ", lines.Skip(1).Take(contactIndex - 1)) : "";
            var contactLine = lines[contactIndex];
            fallbackSummary = string.Join(" ", lines.Skip(contactIndex + 1));

            return (fullName, title, contactLine);
        }

        public static string ExtractEmail(string text)
        {
            var match = Regex.Match(text ?? "", @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
            return match.Success ? match.Value : "";
        }

        public static string ExtractPhone(string text)
        {
            var match = Regex.Match(text ?? "", @"\+?\d[\d\s().-]{7,}\d");
            return match.Success ? match.Value.Trim() : "";
        }

        /// <summary>
        /// Tokenizes a raw "Skills" section (which may have category labels like "Frontend" or "Cloud &
        /// DevOps" ahead of a comma-separated list) into individual skill-ish tokens.
        /// </summary>
        public static List<string> ParseSkillTokens(string skillsText)
        {
            if (string.IsNullOrWhiteSpace(skillsText))
                return new List<string>();

            return Regex.Split(skillsText, "[,\n]")
                .Select(s => s.Trim().TrimEnd('.'))
                .Where(s => s.Length > 1 && s.Length < 40)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
