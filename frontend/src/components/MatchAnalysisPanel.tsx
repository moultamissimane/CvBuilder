'use client';

import { MatchAnalysis, StructuredJobDescription } from '@/lib/api';

interface MatchAnalysisPanelProps {
  analysis: MatchAnalysis;
  job: StructuredJobDescription;
}

export default function MatchAnalysisPanel({ analysis, job }: MatchAnalysisPanelProps) {
  const verified = analysis.skillMatches.filter((m) => m.status === 'Verified');
  const related = analysis.skillMatches.filter((m) => m.status === 'Related');
  const notVerified = analysis.skillMatches.filter((m) => m.status === 'NotVerified');

  return (
    <div className="space-y-6">
      <div className="bg-gradient-to-r from-blue-50 to-indigo-50 p-6 rounded-lg border border-blue-200">
        <div className="flex justify-between items-center mb-4">
          <div>
            <h3 className="text-xl font-bold text-gray-900">Job Match Score</h3>
            {job.jobTitle && <p className="text-sm text-gray-600 mt-1">{job.jobTitle}</p>}
          </div>
          <div className="text-5xl font-bold text-blue-600">{Math.round(analysis.matchScore)}%</div>
        </div>
        <div className="w-full bg-gray-300 rounded-full h-3">
          <div
            className="h-3 rounded-full bg-blue-600 transition-all"
            style={{ width: `${analysis.matchScore}%` }}
          />
        </div>
      </div>

      {verified.length > 0 && (
        <div className="bg-green-50 border border-green-200 rounded-lg p-4">
          <h4 className="font-semibold text-green-900 mb-2">✓ Strong Matches</h4>
          <p className="text-sm text-green-800 mb-3">
            Your CV directly demonstrates these skills.
          </p>
          <div className="flex flex-wrap gap-2">
            {verified.map((m, i) => (
              <span key={i} className="px-3 py-1 bg-green-100 text-green-800 text-sm rounded-full">
                ✓ {m.skill}
              </span>
            ))}
          </div>
        </div>
      )}

      {related.length > 0 && (
        <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
          <h4 className="font-semibold text-amber-900 mb-2">~ Related Experience</h4>
          <p className="text-sm text-amber-800 mb-3">
            The job asks for a general skill your CV shows evidence of, through related technologies.
          </p>
          <div className="flex flex-wrap gap-2">
            {related.map((m, i) => (
              <span key={i} className="px-3 py-1 bg-amber-100 text-amber-800 text-sm rounded-full">
                ~ {m.skill}
              </span>
            ))}
          </div>
        </div>
      )}

      {notVerified.length > 0 && (
        <div className="bg-gray-50 border border-gray-200 rounded-lg p-4">
          <h4 className="font-semibold text-gray-900 mb-2">! Not Verified</h4>
          <p className="text-sm text-gray-700 mb-3">
            The job asks for these, but your CV shows no evidence of them. These will{' '}
            <strong>not</strong> be added to your generated CV — consider whether to genuinely
            learn them before applying.
          </p>
          <div className="flex flex-wrap gap-2">
            {notVerified.map((m, i) => (
              <span key={i} className="px-3 py-1 bg-gray-200 text-gray-700 text-sm rounded-full">
                ! {m.skill}
              </span>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
