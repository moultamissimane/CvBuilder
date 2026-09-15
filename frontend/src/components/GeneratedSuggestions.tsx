'use client';

import { CVGenerationResponse } from '@/lib/api';

interface GeneratedSuggestionsProps {
  suggestions: CVGenerationResponse;
}

export default function GeneratedSuggestions({ suggestions }: GeneratedSuggestionsProps) {
  const getScoreColor = (score: number) => {
    if (score >= 80) return 'text-green-600';
    if (score >= 60) return 'text-yellow-600';
    return 'text-red-600';
  };

  const getScoreBg = (score: number) => {
    if (score >= 80) return 'bg-green-100';
    if (score >= 60) return 'bg-yellow-100';
    return 'bg-red-100';
  };

  return (
    <div className="mt-8 space-y-6 border-t-2 border-gray-300 pt-6">
      <h3 className="text-2xl font-bold text-gray-900">AI-Powered Suggestions</h3>

      {/* Match Score */}
      <div className={`p-6 rounded-lg ${getScoreBg(suggestions.matchScore)}`}>
        <div className="flex items-center justify-between">
          <div>
            <h4 className="text-lg font-semibold text-gray-900">Job Match Score</h4>
            <p className="text-sm text-gray-600 mt-1">
              How well your CV matches the job description
            </p>
          </div>
          <div className={`text-5xl font-bold ${getScoreColor(suggestions.matchScore)}`}>
            {suggestions.matchScore.toFixed(0)}%
          </div>
        </div>
        <div className="w-full bg-gray-300 rounded-full h-2 mt-4">
          <div
            className={`h-2 rounded-full transition-all ${
              suggestions.matchScore >= 80
                ? 'bg-green-600'
                : suggestions.matchScore >= 60
                ? 'bg-yellow-600'
                : 'bg-red-600'
            }`}
            style={{ width: `${suggestions.matchScore}%` }}
          />
        </div>
      </div>

      {/* Tailored Summary */}
      <div>
        <h4 className="text-lg font-semibold text-gray-900 mb-3">Suggested Professional Summary</h4>
        <div className="p-4 bg-blue-50 border border-blue-200 rounded-lg">
          <p className="text-gray-700 leading-relaxed">{suggestions.tailoredSummary}</p>
        </div>
      </div>

      {/* Key Skills to Highlight */}
      <div>
        <h4 className="text-lg font-semibold text-gray-900 mb-3">Key Skills to Highlight</h4>
        <div className="flex flex-wrap gap-2">
          {suggestions.keySkillsToHighlight.map((skill, index) => (
            <span
              key={index}
              className="px-4 py-2 bg-indigo-100 text-indigo-700 rounded-full font-medium text-sm"
            >
              ⭐ {skill}
            </span>
          ))}
        </div>
      </div>

      {/* Relevant Experiences */}
      {suggestions.relevantExperiences.length > 0 && (
        <div>
          <h4 className="text-lg font-semibold text-gray-900 mb-3">Most Relevant Experiences</h4>
          <div className="space-y-2">
            {suggestions.relevantExperiences.map((experience, index) => (
              <div
                key={index}
                className="p-3 bg-green-50 border border-green-200 rounded-lg text-green-900"
              >
                ✓ {experience}
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Recommendations */}
      <div>
        <h4 className="text-lg font-semibold text-gray-900 mb-3">Recommendations to Improve</h4>
        <div className="space-y-2">
          {suggestions.recommendations.map((recommendation, index) => (
            <div
              key={index}
              className="p-3 bg-amber-50 border border-amber-200 rounded-lg text-amber-900"
            >
              💡 {recommendation}
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
