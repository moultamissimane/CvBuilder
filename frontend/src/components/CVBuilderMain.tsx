'use client';

import { useState } from 'react';
import { CVData, api, CVGenerationResponse } from '@/lib/api';
import JobDescriptionInput from './JobDescriptionInput';
import CVForm from './CVForm';
import CVPreview from './CVPreview';
import GeneratedSuggestions from './GeneratedSuggestions';

export default function CVBuilderMain() {
  const [jobDescription, setJobDescription] = useState('');
  const [cv, setCV] = useState<CVData>({
    title: 'My CV',
    fullName: '',
    email: '',
    phone: '',
    summary: '',
    experiences: [],
    education: [],
    skills: [],
    templateId: 'modern',
  });
  const [generatedSuggestions, setGeneratedSuggestions] = useState<CVGenerationResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState<'input' | 'edit' | 'preview'>('input');

  const handleGenerateCV = async () => {
    if (!jobDescription.trim()) {
      setError('Please paste a job description');
      return;
    }

    if (!cv.fullName || !cv.email) {
      setError('Please fill in at least your name and email');
      return;
    }

    setLoading(true);
    setError('');

    try {
      const suggestions = await api.generateTailoredCV(jobDescription, cv);
      setGeneratedSuggestions(suggestions);
      setActiveTab('preview');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateCV = (updatedCV: CVData) => {
    setCV(updatedCV);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <div className="container mx-auto px-4 py-8">
        <div className="mb-8 text-center">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">CV Builder</h1>
          <p className="text-gray-600">Tailor your CV to any job description</p>
        </div>

        {error && (
          <div className="mb-6 p-4 bg-red-100 border border-red-400 text-red-700 rounded">
            {error}
          </div>
        )}

        {/* Tabs */}
        <div className="mb-6 flex gap-2 border-b border-gray-200">
          <button
            onClick={() => setActiveTab('input')}
            className={`px-4 py-2 font-medium transition-colors ${
              activeTab === 'input'
                ? 'text-blue-600 border-b-2 border-blue-600'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            Job Description
          </button>
          <button
            onClick={() => setActiveTab('edit')}
            className={`px-4 py-2 font-medium transition-colors ${
              activeTab === 'edit'
                ? 'text-blue-600 border-b-2 border-blue-600'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            Your CV
          </button>
          <button
            onClick={() => setActiveTab('preview')}
            className={`px-4 py-2 font-medium transition-colors ${
              activeTab === 'preview'
                ? 'text-blue-600 border-b-2 border-blue-600'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            Preview
          </button>
        </div>

        {/* Tab Content */}
        <div className="bg-white rounded-lg shadow-lg p-6 mb-6">
          {activeTab === 'input' && (
            <div className="space-y-4">
              <JobDescriptionInput value={jobDescription} onChange={setJobDescription} />
              <button
                onClick={handleGenerateCV}
                disabled={loading}
                className="w-full px-4 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 transition-colors font-medium"
              >
                {loading ? 'Generating...' : '✨ Generate Tailored CV'}
              </button>
            </div>
          )}

          {activeTab === 'edit' && (
            <CVForm cv={cv} onChange={handleUpdateCV} />
          )}

          {activeTab === 'preview' && (
            <>
              <CVPreview cv={cv} />
              {generatedSuggestions && (
                <GeneratedSuggestions suggestions={generatedSuggestions} />
              )}
            </>
          )}
        </div>
      </div>
    </div>
  );
}
