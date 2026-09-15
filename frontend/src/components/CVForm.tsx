'use client';

import { CVData, Experience, Education } from '@/lib/api';

interface CVFormProps {
  cv: CVData;
  onChange: (cv: CVData) => void;
}

export default function CVForm({ cv, onChange }: CVFormProps) {
  const updateField = (field: keyof CVData, value: any) => {
    onChange({ ...cv, [field]: value });
  };

  const addExperience = () => {
    const newExperience: Experience = {
      companyName: '',
      jobTitle: '',
      startDate: new Date().toISOString().split('T')[0],
      endDate: '',
      currentlyWorking: false,
      description: '',
      achievements: [],
    };
    updateField('experiences', [...cv.experiences, newExperience]);
  };

  const updateExperience = (index: number, experience: Experience) => {
    const experiences = [...cv.experiences];
    experiences[index] = experience;
    updateField('experiences', experiences);
  };

  const removeExperience = (index: number) => {
    updateField('experiences', cv.experiences.filter((_, i) => i !== index));
  };

  const addEducation = () => {
    const newEducation: Education = {
      institution: '',
      degree: '',
      fieldOfStudy: '',
      graduationYear: new Date().getFullYear(),
      grade: '',
    };
    updateField('education', [...cv.education, newEducation]);
  };

  const updateEducation = (index: number, education: Education) => {
    const educations = [...cv.education];
    educations[index] = education;
    updateField('education', educations);
  };

  const removeEducation = (index: number) => {
    updateField('education', cv.education.filter((_, i) => i !== index));
  };

  return (
    <div className="space-y-6">
      {/* Personal Info */}
      <div>
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Personal Information</h3>
        <div className="grid grid-cols-2 gap-4">
          <input
            type="text"
            placeholder="Full Name"
            value={cv.fullName}
            onChange={(e) => updateField('fullName', e.target.value)}
            className="col-span-2 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
          <input
            type="email"
            placeholder="Email"
            value={cv.email}
            onChange={(e) => updateField('email', e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
          <input
            type="tel"
            placeholder="Phone"
            value={cv.phone}
            onChange={(e) => updateField('phone', e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          />
          <textarea
            placeholder="Professional Summary"
            value={cv.summary}
            onChange={(e) => updateField('summary', e.target.value)}
            className="col-span-2 px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent h-24 resize-none"
          />
        </div>
      </div>

      {/* Skills */}
      <div>
        <h3 className="text-lg font-semibold text-gray-900 mb-4">Skills</h3>
        <textarea
          placeholder="Enter skills separated by commas (e.g., JavaScript, React, Node.js)"
          value={cv.skills.join(', ')}
          onChange={(e) => updateField('skills', e.target.value.split(',').map(s => s.trim()).filter(Boolean))}
          className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent h-20 resize-none"
        />
      </div>

      {/* Experience */}
      <div>
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-lg font-semibold text-gray-900">Experience</h3>
          <button
            onClick={addExperience}
            className="px-3 py-1 bg-blue-100 text-blue-600 rounded hover:bg-blue-200 transition-colors text-sm"
          >
            + Add Experience
          </button>
        </div>
        <div className="space-y-4">
          {cv.experiences.map((exp, index) => (
            <div key={index} className="p-4 bg-gray-50 rounded-lg space-y-2 border border-gray-200">
              <input
                type="text"
                placeholder="Job Title"
                value={exp.jobTitle}
                onChange={(e) => updateExperience(index, { ...exp, jobTitle: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
              <input
                type="text"
                placeholder="Company Name"
                value={exp.companyName}
                onChange={(e) => updateExperience(index, { ...exp, companyName: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
              <div className="grid grid-cols-2 gap-2">
                <input
                  type="date"
                  value={exp.startDate}
                  onChange={(e) => updateExperience(index, { ...exp, startDate: e.target.value })}
                  className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
                />
                <input
                  type="date"
                  value={exp.endDate || ''}
                  onChange={(e) => updateExperience(index, { ...exp, endDate: e.target.value })}
                  disabled={exp.currentlyWorking}
                  className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:bg-gray-100"
                />
              </div>
              <label className="flex items-center gap-2 text-sm">
                <input
                  type="checkbox"
                  checked={exp.currentlyWorking}
                  onChange={(e) => updateExperience(index, { ...exp, currentlyWorking: e.target.checked })}
                  className="rounded"
                />
                Currently working here
              </label>
              <textarea
                placeholder="Description and achievements"
                value={exp.description}
                onChange={(e) => updateExperience(index, { ...exp, description: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent h-20 resize-none"
              />
              <button
                onClick={() => removeExperience(index)}
                className="px-3 py-1 bg-red-100 text-red-600 rounded hover:bg-red-200 transition-colors text-sm"
              >
                Remove
              </button>
            </div>
          ))}
        </div>
      </div>

      {/* Education */}
      <div>
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-lg font-semibold text-gray-900">Education</h3>
          <button
            onClick={addEducation}
            className="px-3 py-1 bg-blue-100 text-blue-600 rounded hover:bg-blue-200 transition-colors text-sm"
          >
            + Add Education
          </button>
        </div>
        <div className="space-y-4">
          {cv.education.map((edu, index) => (
            <div key={index} className="p-4 bg-gray-50 rounded-lg space-y-2 border border-gray-200">
              <input
                type="text"
                placeholder="Institution"
                value={edu.institution}
                onChange={(e) => updateEducation(index, { ...edu, institution: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
              <input
                type="text"
                placeholder="Degree"
                value={edu.degree}
                onChange={(e) => updateEducation(index, { ...edu, degree: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
              <input
                type="text"
                placeholder="Field of Study"
                value={edu.fieldOfStudy}
                onChange={(e) => updateEducation(index, { ...edu, fieldOfStudy: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
              <input
                type="number"
                placeholder="Graduation Year"
                value={edu.graduationYear}
                onChange={(e) => updateEducation(index, { ...edu, graduationYear: parseInt(e.target.value) })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              />
              <button
                onClick={() => removeEducation(index)}
                className="px-3 py-1 bg-red-100 text-red-600 rounded hover:bg-red-200 transition-colors text-sm"
              >
                Remove
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
