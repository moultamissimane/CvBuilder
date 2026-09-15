'use client';

import { useState } from 'react';
import { api, EnhancedCVResponse } from '@/lib/api';
import CVInput from './CVInput';
import JobDescriptionInput from './JobDescriptionInput';
import EnhancedCVDisplay from './EnhancedCVDisplay';

export default function CVEnhancerMain() {
  const [jobDescription, setJobDescription] = useState('');
  const [enhancedCV, setEnhancedCV] = useState<EnhancedCVResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [activeTab, setActiveTab] = useState<'input' | 'result'>('input');

  const handleEnhanceCV = async (cvInput: string | File, inputType: 'pdf' | 'text') => {
    if (!jobDescription.trim()) {
      setError('Please enter a job description first');
      return;
    }

    setLoading(true);
    setError('');

    try {
      let result;

      if (inputType === 'pdf') {
        result = await api.enhanceCVFromPDF(cvInput as File, jobDescription);
      } else {
        result = await api.enhanceCVFromText(cvInput as string, jobDescription);
      }

      setEnhancedCV(result);
      setActiveTab('result');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred while enhancing your CV');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 py-8">
      <div className="container mx-auto px-4">
        <div className="mb-8 text-center">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">AI CV Enhancer</h1>
          <p className="text-gray-600">Transform your CV to match any job description</p>
        </div>

        {error && (
          <div className="mb-6 p-4 bg-red-100 border border-red-400 text-red-700 rounded-lg">
            {error}
          </div>
        )}

        {/* Tabs */}
        <div className="mb-6 flex gap-2 border-b border-gray-300">
          <button
            onClick={() => setActiveTab('input')}
            className={`px-4 py-3 font-medium transition-colors ${
              activeTab === 'input'
                ? 'text-blue-600 border-b-2 border-blue-600'
                : 'text-gray-600 hover:text-gray-900'
            }`}
          >
            Enter Your Information
          </button>
          <button
            onClick={() => setActiveTab('result')}
            disabled={!enhancedCV}
            className={`px-4 py-3 font-medium transition-colors ${
              activeTab === 'result'
                ? 'text-blue-600 border-b-2 border-blue-600'
                : 'text-gray-600 hover:text-gray-900 disabled:text-gray-400 disabled:cursor-not-allowed'
            }`}
          >
            Your Enhanced CV
          </button>
        </div>

        {/* Content */}
        <div className="bg-white rounded-lg shadow-lg p-6 md:p-8">
          {activeTab === 'input' ? (
            <div className="space-y-8">
              {/* Job Description */}
              <div>
                <h2 className="text-2xl font-semibold text-gray-900 mb-4">Step 1: Paste Job Description</h2>
                <JobDescriptionInput value={jobDescription} onChange={setJobDescription} />
              </div>

              <div className="border-t border-gray-300 pt-8">
                {/* CV Input */}
                <div>
                  <h2 className="text-2xl font-semibold text-gray-900 mb-4">Step 2: Upload or Paste Your CV</h2>
                  <CVInput onCVSubmit={handleEnhanceCV} loading={loading} />
                </div>
              </div>

              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <h3 className="font-semibold text-blue-900 mb-2">💡 How It Works</h3>
                <ul className="text-sm text-blue-800 space-y-1">
                  <li>✓ Our AI analyzes the job description to identify required skills</li>
                  <li>✓ Your CV is enhanced with matching skills and experience</li>
                  <li>✓ Missing skills are added as "Familiar with" to show learning potential</li>
                  <li>✓ A proposed project is added to demonstrate capability</li>
                </ul>
              </div>
            </div>
          ) : (
            enhancedCV && <EnhancedCVDisplay enhancedCV={enhancedCV} />
          )}
        </div>
      </div>

      {/* Print Styles */}
      <style>{`
        @media print {
          body {
            background: white;
            padding: 0;
          }
          .container {
            max-width: 100%;
          }
          button, .border-b, .tabs {
            display: none;
          }
          .bg-white {
            box-shadow: none;
            border: none;
          }
        }
      `}</style>
    </div>
  );
}
