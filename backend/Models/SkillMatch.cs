namespace CVBuilder.API.Models
{
    public enum SkillMatchStatus
    {
        /// <summary>The CV directly demonstrates this skill (verbatim or a known synonym).</summary>
        Verified,

        /// <summary>The job asked for a general concept (e.g. "REST APIs", "cloud experience") and the
        /// CV has concrete evidence of that concept — never used to imply the user knows a specific
        /// named technology they haven't demonstrated.</summary>
        Related,

        /// <summary>The job asks for it; the CV shows no evidence of it. Never surfaced as something the
        /// user "has" anywhere in the generated CV — analysis/UI only.</summary>
        NotVerified
    }

    public class SkillMatch
    {
        public string Skill { get; set; } = "";
        public SkillMatchStatus Status { get; set; }
        public bool IsPreferred { get; set; }

        /// <summary>For Related matches: what in the CV justified the classification.</summary>
        public string Evidence { get; set; } = "";
    }

    public class MatchAnalysis
    {
        public List<SkillMatch> SkillMatches { get; set; } = new();
        public double MatchScore { get; set; }
        public int RequiredSkillCount { get; set; }
        public int VerifiedCount { get; set; }
        public int RelatedCount { get; set; }
        public int NotVerifiedCount { get; set; }
    }
}
