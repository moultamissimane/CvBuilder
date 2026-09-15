'use client';

import { EnhancedCVResponse } from '@/lib/api';

interface EnhancedCVDocumentProps {
  enhancedCV: EnhancedCVResponse;
}

export default function EnhancedCVDocument({ enhancedCV }: EnhancedCVDocumentProps) {
  return (
    <div className="bg-white p-8 border border-gray-300 rounded-lg print:border-0 print:rounded-0">
      {/* Header */}
      <div className="mb-6 text-center border-b-2 border-gray-300 pb-6">
        <h1 className="text-3xl font-bold text-gray-900">
          {enhancedCV.fullName || 'Your Name'}
        </h1>
        {enhancedCV.title && (
          <p className="text-gray-700 text-sm mt-1">{enhancedCV.title}</p>
        )}
        {enhancedCV.contactLine && (
          <p className="text-gray-600 text-sm mt-1">{enhancedCV.contactLine}</p>
        )}
      </div>

      {/* Summary — the user's own summary, lightly extended with job-relevant keywords */}
      {enhancedCV.summary && (
        <div className="mb-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-2 uppercase tracking-wide">
            Professional Summary
          </h2>
          <p className="text-gray-700 text-sm leading-relaxed">{enhancedCV.summary}</p>
        </div>
      )}

      {/* Experience — the user's own experience section, kept exactly as written */}
      {(enhancedCV.experienceText || enhancedCV.syntheticExperience) && (
        <div className="mb-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-3 uppercase tracking-wide">
            Professional Experience
          </h2>
          {enhancedCV.experienceText && (
            <p className="text-sm text-gray-700 leading-relaxed whitespace-pre-wrap">
              {enhancedCV.experienceText}
            </p>
          )}

          {/* New entry proposed by the AI, appended after the user's real experience — never mixed in */}
          {enhancedCV.syntheticExperience && (
            <div className="mt-4 pl-3 border-l-2 border-amber-300">
              <h3 className="text-base font-semibold text-gray-900">
                {enhancedCV.syntheticExperience.title}
              </h3>
              <p className="text-sm text-gray-700 mt-1 leading-relaxed">
                {enhancedCV.syntheticExperience.description}
              </p>
            </div>
          )}
        </div>
      )}

      {/* Education — preserved exactly as written */}
      {enhancedCV.educationText && (
        <div className="mb-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-3 uppercase tracking-wide">
            Education
          </h2>
          <p className="text-sm text-gray-700 leading-relaxed whitespace-pre-wrap">
            {enhancedCV.educationText}
          </p>
        </div>
      )}

      {/* Certifications — preserved exactly as written */}
      {enhancedCV.certificationsText && (
        <div className="mb-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-3 uppercase tracking-wide">
            Certifications
          </h2>
          <p className="text-sm text-gray-700 leading-relaxed whitespace-pre-wrap">
            {enhancedCV.certificationsText}
          </p>
        </div>
      )}

      {/* Skills — the user's own skills list, with any job-matched additions appended below it */}
      {(enhancedCV.skillsText || enhancedCV.addedSkills.length > 0) && (
        <div className="mb-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-2 uppercase tracking-wide">
            Skills
          </h2>
          {enhancedCV.skillsText && (
            <p className="text-sm text-gray-700 leading-relaxed whitespace-pre-wrap">
              {enhancedCV.skillsText}
            </p>
          )}

          {enhancedCV.addedSkills.length > 0 && (
            <div className="mt-3">
              <p className="text-xs font-semibold text-amber-800 uppercase tracking-wide mb-1">
                Additional Skills (matched to job)
              </p>
              <div className="flex flex-wrap gap-2">
                {enhancedCV.addedSkills.map((skill, index) => (
                  <span
                    key={index}
                    className="px-3 py-1 text-sm rounded-full bg-amber-100 text-amber-800"
                  >
                    {skill.skill} <span className="text-xs">(Familiar with)</span>
                  </span>
                ))}
              </div>
            </div>
          )}
        </div>
      )}

      {/* Languages — preserved exactly as written */}
      {enhancedCV.languagesText && (
        <div className="mb-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-3 uppercase tracking-wide">
            Languages
          </h2>
          <p className="text-sm text-gray-700 leading-relaxed whitespace-pre-wrap">
            {enhancedCV.languagesText}
          </p>
        </div>
      )}
    </div>
  );
}
