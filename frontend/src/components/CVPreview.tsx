'use client';

import { CVData } from '@/lib/api';

interface CVPreviewProps {
  cv: CVData;
}

export default function CVPreview({ cv }: CVPreviewProps) {
  const handlePrint = () => {
    window.print();
  };

  return (
    <div className="space-y-4">
      <button
        onClick={handlePrint}
        className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 transition-colors font-medium"
      >
        🖨️ Print / Save as PDF
      </button>

      <div className="bg-white p-8 border border-gray-300 rounded-lg print:border-0 print:rounded-0">
        {/* Header */}
        <div className="mb-6 text-center border-b-2 border-gray-300 pb-6">
          <h1 className="text-3xl font-bold text-gray-900">{cv.fullName}</h1>
          <p className="text-gray-600 text-sm mt-1">
            {cv.email} {cv.phone && `• ${cv.phone}`}
          </p>
        </div>

        {/* Summary */}
        {cv.summary && (
          <div className="mb-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-2 uppercase tracking-wide">
              Professional Summary
            </h2>
            <p className="text-gray-700 text-sm leading-relaxed">{cv.summary}</p>
          </div>
        )}

        {/* Skills */}
        {cv.skills.length > 0 && (
          <div className="mb-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-2 uppercase tracking-wide">
              Skills
            </h2>
            <div className="flex flex-wrap gap-2">
              {cv.skills.map((skill, index) => (
                <span
                  key={index}
                  className="px-3 py-1 bg-blue-100 text-blue-700 text-sm rounded-full"
                >
                  {skill}
                </span>
              ))}
            </div>
          </div>
        )}

        {/* Experience */}
        {cv.experiences.length > 0 && (
          <div className="mb-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-3 uppercase tracking-wide">
              Experience
            </h2>
            <div className="space-y-4">
              {cv.experiences.map((exp, index) => (
                <div key={index}>
                  <div className="flex justify-between items-start mb-1">
                    <h3 className="text-base font-semibold text-gray-900">{exp.jobTitle}</h3>
                    <span className="text-sm text-gray-600">
                      {new Date(exp.startDate).toLocaleDateString('en-US', {
                        month: 'short',
                        year: 'numeric',
                      })}
                      {' - '}
                      {exp.currentlyWorking
                        ? 'Present'
                        : new Date(exp.endDate || '').toLocaleDateString('en-US', {
                          month: 'short',
                          year: 'numeric',
                        })}
                    </span>
                  </div>
                  <p className="text-sm text-gray-700 font-medium">{exp.companyName}</p>
                  <p className="text-sm text-gray-600 mt-1 whitespace-pre-wrap">{exp.description}</p>
                </div>
              ))}
            </div>
          </div>
        )}

        {/* Education */}
        {cv.education.length > 0 && (
          <div className="mb-6">
            <h2 className="text-lg font-semibold text-gray-900 mb-3 uppercase tracking-wide">
              Education
            </h2>
            <div className="space-y-3">
              {cv.education.map((edu, index) => (
                <div key={index}>
                  <div className="flex justify-between items-start">
                    <div>
                      <h3 className="text-base font-semibold text-gray-900">{edu.degree}</h3>
                      <p className="text-sm text-gray-700">{edu.institution}</p>
                      <p className="text-sm text-gray-600">{edu.fieldOfStudy}</p>
                    </div>
                    <span className="text-sm text-gray-600">{edu.graduationYear}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      <style>{`
        @media print {
          body {
            margin: 0;
            padding: 0;
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
