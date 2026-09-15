namespace CVBuilder.API.Services
{
    /// <summary>
    /// A curated catalog of common technology names, with light alias handling (e.g. "Node" / "NodeJS" /
    /// "Node.js" all resolve to the same canonical term). This powers both job-skill extraction and the
    /// "does the CV actually demonstrate this?" check in the skill matching engine — it is intentionally
    /// not exhaustive; unrecognized terms simply fall back to the phrase-based extractor.
    /// </summary>
    public static class TechKeywords
    {
        // Canonical name -> aliases (canonical name is always included as its own alias).
        public static readonly Dictionary<string, string[]> Catalog = new(StringComparer.OrdinalIgnoreCase)
        {
            ["React"] = new[] { "React", "React.js", "ReactJS" },
            ["Next.js"] = new[] { "Next.js", "NextJS", "Next" },
            ["Angular"] = new[] { "Angular", "AngularJS", "Angular.js" },
            ["Vue.js"] = new[] { "Vue.js", "VueJS", "Vue" },
            ["Svelte"] = new[] { "Svelte", "SvelteKit" },
            ["TypeScript"] = new[] { "TypeScript", "TS" },
            // "JS" is deliberately not an alias here: it false-matches as a substring of "Next.js" /
            // "Node.js" (the trailing ".js" reads as a standalone "JS" token to a boundary-only regex).
            // Missing an occasional CV that only ever writes "JS" is the safe failure direction; spuriously
            // treating every Next.js/Node.js mention as also claiming JavaScript is not.
            ["JavaScript"] = new[] { "JavaScript", "ECMAScript" },
            ["Redux"] = new[] { "Redux" },
            ["Tailwind CSS"] = new[] { "Tailwind CSS", "Tailwind", "TailwindCSS" },
            ["HTML"] = new[] { "HTML", "HTML5" },
            ["CSS"] = new[] { "CSS", "CSS3" },
            ["Sass"] = new[] { "Sass", "SCSS" },
            ["jQuery"] = new[] { "jQuery" },
            ["Material UI"] = new[] { "Material UI", "MUI" },

            ["Node.js"] = new[] { "Node.js", "NodeJS", "Node" },
            ["Express.js"] = new[] { "Express.js", "ExpressJS", "Express" },
            ["NestJS"] = new[] { "NestJS", "Nest.js" },
            [".NET"] = new[] { ".NET", "ASP.NET", ".NET Core", "dotnet" },
            ["C#"] = new[] { "C#", "CSharp" },
            ["Java"] = new[] { "Java" },
            ["Spring"] = new[] { "Spring", "Spring Boot" },
            ["Python"] = new[] { "Python" },
            ["Django"] = new[] { "Django" },
            ["Flask"] = new[] { "Flask" },
            ["PHP"] = new[] { "PHP" },
            ["Laravel"] = new[] { "Laravel" },
            ["Ruby"] = new[] { "Ruby", "Ruby on Rails", "Rails" },
            ["Go"] = new[] { "Golang", "Go" },
            ["Rust"] = new[] { "Rust" },
            ["GraphQL"] = new[] { "GraphQL" },
            ["REST API"] = new[] { "REST API", "RESTful API", "REST", "RESTful" },
            ["gRPC"] = new[] { "gRPC" },

            ["PostgreSQL"] = new[] { "PostgreSQL", "Postgres" },
            ["MySQL"] = new[] { "MySQL" },
            ["MongoDB"] = new[] { "MongoDB", "Mongo" },
            ["SQL Server"] = new[] { "SQL Server", "MSSQL" },
            ["SQLite"] = new[] { "SQLite" },
            ["Redis"] = new[] { "Redis" },
            ["Elasticsearch"] = new[] { "Elasticsearch" },
            ["SQL"] = new[] { "SQL" },
            ["NoSQL"] = new[] { "NoSQL" },
            ["Prisma"] = new[] { "Prisma" },

            ["AWS"] = new[] { "AWS", "Amazon Web Services" },
            ["Azure"] = new[] { "Azure", "Microsoft Azure" },
            ["GCP"] = new[] { "GCP", "Google Cloud", "Google Cloud Platform" },
            ["Firebase"] = new[] { "Firebase" },
            ["Vercel"] = new[] { "Vercel" },
            ["Netlify"] = new[] { "Netlify" },

            ["Docker"] = new[] { "Docker" },
            ["Kubernetes"] = new[] { "Kubernetes", "K8s" },
            ["Terraform"] = new[] { "Terraform" },
            ["Jenkins"] = new[] { "Jenkins" },
            ["GitHub Actions"] = new[] { "GitHub Actions" },
            ["GitLab CI"] = new[] { "GitLab CI", "GitLab CI/CD" },
            ["CI/CD"] = new[] { "CI/CD", "Continuous Integration", "Continuous Deployment" },
            ["Git"] = new[] { "Git", "GitHub", "GitLab", "Bitbucket" },
            ["Linux"] = new[] { "Linux" },
            ["Nginx"] = new[] { "Nginx" },

            ["React Native"] = new[] { "React Native" },
            ["Flutter"] = new[] { "Flutter" },
            ["Swift"] = new[] { "Swift" },
            ["Kotlin"] = new[] { "Kotlin" },

            ["Figma"] = new[] { "Figma" },
            ["Jira"] = new[] { "Jira" },
            ["Agile"] = new[] { "Agile" },
            ["Scrum"] = new[] { "Scrum" },
            ["Jest"] = new[] { "Jest" },
            ["Cypress"] = new[] { "Cypress" },
            ["WordPress"] = new[] { "WordPress" },
            ["Shopify"] = new[] { "Shopify" },
        };

        /// <summary>
        /// Returns the canonical catalog names found as whole-word (case-insensitive) matches anywhere in
        /// the given text, preserving catalog order for determinism.
        /// </summary>
        public static List<string> ExtractKnownMentions(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            var found = new List<string>();
            foreach (var (canonical, aliases) in Catalog)
            {
                if (aliases.Any(alias => ContainsWholeWord(text, alias)))
                {
                    found.Add(canonical);
                }
            }

            return found;
        }

        public static bool ContainsWholeWord(string text, string term)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(term))
                return false;

            // '.' is intentionally NOT excluded here — "React" must still match inside "React.js", and
            // "Vue" inside "Vue.js", since those dotted forms are exactly how people write them. The
            // narrower problem (a short alias like "JS" false-matching inside "Next.js"/"Node.js") is
            // handled by not registering "JS" as an alias at all rather than by punishing every term.
            var pattern = @"(?<![A-Za-z0-9])" + System.Text.RegularExpressions.Regex.Escape(term) + @"(?![A-Za-z0-9])";
            return System.Text.RegularExpressions.Regex.IsMatch(text, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        /// <summary>Resolves any alias to its canonical catalog name, or returns the input unchanged if unknown.</summary>
        public static string Canonicalize(string term)
        {
            foreach (var (canonical, aliases) in Catalog)
            {
                if (aliases.Any(a => string.Equals(a, term, StringComparison.OrdinalIgnoreCase)))
                    return canonical;
            }
            return term;
        }
    }
}
