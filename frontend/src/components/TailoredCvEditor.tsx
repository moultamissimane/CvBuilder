'use client';

import { StructuredCv, StructuredExperience, StructuredEducation, SkillGroups } from '@/lib/api';

interface TailoredCvEditorProps {
  cv: StructuredCv;
  onChange: (cv: StructuredCv) => void;
  onExportPdf: () => void;
  onSave: () => void;
  exporting: boolean;
  saving: boolean;
}

const SKILL_CATEGORIES: { key: keyof SkillGroups; label: string }[] = [
  { key: 'frontend', label: 'Frontend' },
  { key: 'backend', label: 'Backend' },
  { key: 'databases', label: 'Databases' },
  { key: 'cloud', label: 'Cloud' },
  { key: 'devOps', label: 'DevOps' },
  { key: 'tools', label: 'Tools' },
  { key: 'other', label: 'Other' },
];

// Inline-editable, borderless-until-focus fields so the editor reads like a document rather than a form.
const fieldClass =
  'w-full bg-transparent border border-transparent hover:border-gray-200 focus:border-blue-400 focus:bg-blue-50/40 rounded px-2 py-1 outline-none transition-colors';

export default function TailoredCvEditor({ cv, onChange, onExportPdf, onSave, exporting, saving }: TailoredCvEditorProps) {
  const update = (patch: Partial<StructuredCv>) => onChange({ ...cv, ...patch });

  const updateExperience = (index: number, patch: Partial<StructuredExperience>) => {
    const next = [...cv.experience];
    next[index] = { ...next[index], ...patch };
    update({ experience: next });
  };

  const updateEducation = (index: number, patch: Partial<StructuredEducation>) => {
    const next = [...cv.education];
    next[index] = { ...next[index], ...patch };
    update({ education: next });
  };

  const updateSkillCategory = (key: keyof SkillGroups, text: string) => {
    const items = text.split(',').map((s) => s.trim()).filter(Boolean);
    update({ skills: { ...cv.skills, [key]: items } });
  };

  return (
    <div className="space-y-4">
      <div className="flex gap-3 no-print">
        <button
          onClick={onExportPdf}
          disabled={exporting}
          className="flex-1 px-4 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 transition-colors font-medium"
        >
          {exporting ? 'Generating PDF...' : '⬇️ Download PDF'}
        </button>
        <button
          onClick={onSave}
          disabled={saving}
          className="flex-1 px-4 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 disabled:bg-gray-400 transition-colors font-medium"
        >
          {saving ? 'Saving...' : '💾 Save This Version'}
        </button>
      </div>

      <div id="tailored-cv-document" className="bg-white p-8 border border-gray-300 rounded-lg">
        {/* Header */}
        <div className="mb-6 text-center border-b-2 border-gray-300 pb-6">
          <input
            className={`${fieldClass} text-3xl font-bold text-gray-900 text-center`}
            value={cv.personalInfo.fullName}
            onChange={(e) => update({ personalInfo: { ...cv.personalInfo, fullName: e.target.value } })}
          />
          <input
            className={`${fieldClass} text-sm text-gray-700 text-center mt-1`}
            value={cv.personalInfo.title}
            onChange={(e) => update({ personalInfo: { ...cv.personalInfo, title: e.target.value } })}
          />
          <input
            className={`${fieldClass} text-sm text-gray-600 text-center mt-1`}
            value={cv.personalInfo.contactLine}
            onChange={(e) => update({ personalInfo: { ...cv.personalInfo, contactLine: e.target.value } })}
          />
        </div>

        {/* Summary */}
        <Section title="Professional Summary">
          <textarea
            className={`${fieldClass} text-sm text-gray-700 leading-relaxed`}
            rows={4}
            value={cv.summary}
            onChange={(e) => update({ summary: e.target.value })}
          />
        </Section>

        {/* Experience */}
        {cv.experience.length > 0 ? (
          <Section title="Professional Experience">
            <div className="space-y-5">
              {cv.experience.map((exp, i) => (
                <div key={i} className="border-l-2 border-gray-200 pl-3">
                  <div className="flex flex-wrap gap-2 items-baseline">
                    <input
                      className={`${fieldClass} font-semibold text-gray-900 flex-1 min-w-[140px]`}
                      value={exp.jobTitle}
                      onChange={(e) => updateExperience(i, { jobTitle: e.target.value })}
                    />
                    <span className="text-gray-500">—</span>
                    <input
                      className={`${fieldClass} font-semibold text-gray-900 flex-1 min-w-[140px]`}
                      value={exp.company}
                      onChange={(e) => updateExperience(i, { company: e.target.value })}
                    />
                  </div>
                  <input
                    className={`${fieldClass} text-xs text-gray-500`}
                    value={exp.dateRange}
                    onChange={(e) => updateExperience(i, { dateRange: e.target.value })}
                  />
                  {exp.technologies.length > 0 && (
                    <input
                      className={`${fieldClass} text-xs text-gray-500 italic`}
                      value={exp.technologies.join(', ')}
                      onChange={(e) =>
                        updateExperience(i, { technologies: e.target.value.split(',').map((t) => t.trim()).filter(Boolean) })
                      }
                    />
                  )}
                  <textarea
                    className={`${fieldClass} text-sm text-gray-700 mt-1`}
                    rows={Math.max(2, exp.description.length)}
                    value={exp.description.join('\n')}
                    onChange={(e) => updateExperience(i, { description: e.target.value.split('\n') })}
                  />
                </div>
              ))}
            </div>
          </Section>
        ) : cv.rawExperienceText ? (
          <Section title="Professional Experience">
            <textarea
              className={`${fieldClass} text-sm text-gray-700 whitespace-pre-wrap`}
              rows={10}
              value={cv.rawExperienceText}
              onChange={(e) => update({ rawExperienceText: e.target.value })}
            />
          </Section>
        ) : null}

        {/* Education */}
        {cv.education.length > 0 && (
          <Section title="Education">
            <div className="space-y-3">
              {cv.education.map((edu, i) => (
                <div key={i}>
                  <input
                    className={`${fieldClass} font-semibold text-gray-900`}
                    value={edu.degree}
                    onChange={(e) => updateEducation(i, { degree: e.target.value })}
                  />
                  <div className="flex gap-2">
                    <input
                      className={`${fieldClass} text-xs text-gray-500`}
                      value={edu.dateRange}
                      onChange={(e) => updateEducation(i, { dateRange: e.target.value })}
                    />
                    <input
                      className={`${fieldClass} text-xs text-gray-500`}
                      value={edu.institution}
                      onChange={(e) => updateEducation(i, { institution: e.target.value })}
                    />
                  </div>
                </div>
              ))}
            </div>
          </Section>
        )}

        {/* Certifications */}
        {cv.certifications.length > 0 && (
          <Section title="Certifications">
            <textarea
              className={`${fieldClass} text-sm text-gray-700`}
              rows={Math.max(2, cv.certifications.length)}
              value={cv.certifications.join('\n')}
              onChange={(e) => update({ certifications: e.target.value.split('\n') })}
            />
          </Section>
        )}

        {/* Skills */}
        <Section title="Skills">
          <div className="space-y-2">
            {SKILL_CATEGORIES.filter((c) => cv.skills[c.key]?.length > 0).map((c) => (
              <div key={c.key} className="flex gap-2 items-start">
                <span className="text-sm font-semibold text-gray-900 w-24 shrink-0 pt-1">{c.label}:</span>
                <input
                  className={`${fieldClass} text-sm text-gray-700 flex-1`}
                  value={cv.skills[c.key].join(', ')}
                  onChange={(e) => updateSkillCategory(c.key, e.target.value)}
                />
              </div>
            ))}
          </div>
        </Section>

        {/* Projects */}
        {cv.projects.length > 0 && (
          <Section title="Projects">
            <div className="space-y-3">
              {cv.projects.map((p, i) => (
                <div key={i}>
                  <p className="font-semibold text-gray-900 text-sm">{p.name}</p>
                  <p className="text-sm text-gray-700">{p.description}</p>
                  {p.technologies.length > 0 && (
                    <p className="text-xs text-gray-500 italic">{p.technologies.join(', ')}</p>
                  )}
                </div>
              ))}
            </div>
          </Section>
        )}

        {/* Languages */}
        {cv.languages.length > 0 && (
          <Section title="Languages">
            <input
              className={`${fieldClass} text-sm text-gray-700`}
              value={cv.languages.join(', ')}
              onChange={(e) => update({ languages: e.target.value.split(',').map((l) => l.trim()).filter(Boolean) })}
            />
          </Section>
        )}
      </div>
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="mb-6">
      <h2 className="text-lg font-semibold text-gray-900 mb-2 uppercase tracking-wide">{title}</h2>
      {children}
    </div>
  );
}
