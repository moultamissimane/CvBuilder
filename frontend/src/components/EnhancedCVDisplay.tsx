'use client';

import { EnhancedCVResponse } from '@/lib/api';
import EnhancedCVDocument from './EnhancedCVDocument';

interface EnhancedCVDisplayProps {
  enhancedCV: EnhancedCVResponse;
}

export default function EnhancedCVDisplay({ enhancedCV }: EnhancedCVDisplayProps) {
  const handleDownload = () => {
    const content = generatePlainText();
    const blob = new Blob([content], { type: 'text/plain' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${(enhancedCV.fullName || 'enhanced-cv').replace(/\s+/g, '-').toLowerCase()}.txt`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
  };

  const handlePrint = () => {
    window.print();
  };

  const generatePlainText = () => {
    const lines = [
      enhancedCV.fullName || 'Your Name',
      enhancedCV.title,
      enhancedCV.contactLine,
      '',
      'PROFESSIONAL SUMMARY',
      enhancedCV.summary,
      '',
      'PROFESSIONAL EXPERIENCE',
      enhancedCV.experienceText,
    ];

    if (enhancedCV.syntheticExperience) {
      lines.push(
        '',
        enhancedCV.syntheticExperience.title,
        enhancedCV.syntheticExperience.description
      );
    }

    if (enhancedCV.educationText) {
      lines.push('', 'EDUCATION', enhancedCV.educationText);
    }

    if (enhancedCV.certificationsText) {
      lines.push('', 'CERTIFICATIONS', enhancedCV.certificationsText);
    }

    if (enhancedCV.skillsText || enhancedCV.addedSkills.length > 0) {
      lines.push('', 'SKILLS', enhancedCV.skillsText);
      if (enhancedCV.addedSkills.length > 0) {
        lines.push(
          '',
          'Additional Skills (matched to job):',
          enhancedCV.addedSkills.map((skill) => `• ${skill.skill} (Familiar with)`).join('\n')
        );
      }
    }

    if (enhancedCV.languagesText) {
      lines.push('', 'LANGUAGES', enhancedCV.languagesText);
    }

    return lines.filter((l) => l !== undefined && l !== null).join('\n');
  };

  return (
    <div className="space-y-8">
      {/* Action Buttons */}
      <div className="no-print flex gap-3">
        <button
          onClick={handleDownload}
          className="flex-1 px-4 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-medium"
        >
          ⬇️ Download as Text
        </button>
        <button
          onClick={handlePrint}
          className="flex-1 px-4 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium"
        >
          🖨️ Print/Save as PDF
        </button>
      </div>

      {/* The actual CV — this is the only part that gets printed */}
      <div id="enhanced-cv-document">
        <EnhancedCVDocument enhancedCV={enhancedCV} />
      </div>

      {/* AI Analysis — shown on screen only, not included in the print/PDF */}
      <div className="no-print space-y-6">
        <div className="border-t border-gray-300 pt-6">
          <h2 className="text-xl font-bold text-gray-900 mb-4">AI Analysis</h2>
        </div>

        {/* Match Score */}
        <div className="bg-gradient-to-r from-blue-50 to-indigo-50 p-6 rounded-lg border border-blue-200">
          <div className="flex justify-between items-center mb-4">
            <h3 className="text-xl font-bold text-gray-900">Job Match Score</h3>
            <div className="text-5xl font-bold text-blue-600">{Math.round(enhancedCV.matchScore)}%</div>
          </div>
          <div className="w-full bg-gray-300 rounded-full h-3">
            <div
              className="h-3 rounded-full bg-blue-600 transition-all"
              style={{ width: `${enhancedCV.matchScore}%` }}
            />
          </div>
        </div>

        {/* Added Skills Explanation */}
        {enhancedCV.addedSkills.length > 0 && (
          <div className="bg-amber-50 border border-amber-200 rounded-lg p-4">
            <h4 className="font-semibold text-amber-900 mb-2">Skills Added to Match Job Requirements</h4>
            <p className="text-sm text-amber-800 mb-3">
              These skills have been marked as "Familiar with" on your CV to indicate areas where you can
              quickly learn and apply:
            </p>
            <ul className="space-y-1">
              {enhancedCV.addedSkills.map((skill, index) => (
                <li key={index} className="text-sm text-amber-900">
                  • <strong>{skill.skill}</strong> ({skill.level})
                </li>
              ))}
            </ul>
          </div>
        )}

        {/* Synthetic Experience note */}
        {enhancedCV.syntheticExperience && (
          <div className="border-2 border-green-300 bg-green-50 rounded-lg p-4">
            <h4 className="font-semibold text-green-900 mb-1">
              {enhancedCV.syntheticExperience.title} — Proposed to Strengthen Your Profile
            </h4>
            <p className="text-xs text-green-800 italic">
              💡 This project has been added to your CV above. Customize it with your own actual projects
              or learning experiences that use these technologies before sending your CV out.
            </p>
          </div>
        )}

        {/* Recommendations */}
        <div className="space-y-2">
          <h3 className="text-lg font-semibold text-gray-900">Recommendations</h3>
          <div className="space-y-2">
            {enhancedCV.recommendations.map((rec, index) => (
              <div key={index} className="p-3 bg-blue-50 border border-blue-200 rounded-lg text-gray-700">
                {rec}
              </div>
            ))}
          </div>
        </div>
      </div>

      <style>{`
        @media print {
          .no-print {
            display: none !important;
          }
          body * {
            visibility: hidden;
          }
          #enhanced-cv-document,
          #enhanced-cv-document * {
            visibility: visible;
          }
          #enhanced-cv-document {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
          }
          .print\\:border-0 {
            border: 0 !important;
          }
          .print\\:rounded-0 {
            border-radius: 0 !important;
          }
        }
      `}</style>
    </div>
  );
}
