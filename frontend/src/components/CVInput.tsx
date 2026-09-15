'use client';

import { useState } from 'react';

interface CVInputProps {
  onCVSubmit: (cvText: string, inputType: 'pdf' | 'text') => void;
  loading: boolean;
}

export default function CVInput({ onCVSubmit, loading }: CVInputProps) {
  const [inputType, setInputType] = useState<'pdf' | 'text'>('pdf');
  const [cvText, setCVText] = useState('');
  const [fileName, setFileName] = useState('');

  const handleFileUpload = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setFileName(file.name);
      const reader = new FileReader();
      reader.onload = async (event) => {
        const arrayBuffer = event.target?.result as ArrayBuffer;
        // Store the file for later use in the API call
        (window as any).uploadedCVFile = file;
        setCVText(file.name); // Just store filename for now
      };
      reader.readAsArrayBuffer(file);
    }
  };

  const handlePaste = async () => {
    try {
      const text = await navigator.clipboard.readText();
      setCVText(text);
    } catch (err) {
      console.error('Failed to read clipboard:', err);
    }
  };

  const handleSubmit = () => {
    if (inputType === 'pdf') {
      if (!(window as any).uploadedCVFile) {
        alert('Please upload a PDF file');
        return;
      }
      onCVSubmit((window as any).uploadedCVFile, 'pdf');
    } else {
      if (!cvText.trim()) {
        alert('Please paste your CV or upload a document');
        return;
      }
      onCVSubmit(cvText, 'text');
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex gap-4">
        <label className="flex items-center gap-2">
          <input
            type="radio"
            checked={inputType === 'pdf'}
            onChange={() => setInputType('pdf')}
            className="rounded"
          />
          <span>Upload PDF</span>
        </label>
        <label className="flex items-center gap-2">
          <input
            type="radio"
            checked={inputType === 'text'}
            onChange={() => setInputType('text')}
            className="rounded"
          />
          <span>Paste Text</span>
        </label>
      </div>

      {inputType === 'pdf' ? (
        <div className="space-y-3">
          <label className="block">
            <div className="px-4 py-6 border-2 border-dashed border-gray-300 rounded-lg text-center cursor-pointer hover:border-blue-500 hover:bg-blue-50 transition-colors">
              <div className="text-gray-600">
                <p className="text-lg font-medium">📄 Upload Your CV</p>
                <p className="text-sm text-gray-500 mt-1">Click to select a PDF file</p>
                {fileName && <p className="text-sm text-green-600 mt-2">✓ {fileName}</p>}
              </div>
              <input
                type="file"
                accept=".pdf"
                onChange={handleFileUpload}
                className="hidden"
              />
            </div>
          </label>
        </div>
      ) : (
        <div className="space-y-3">
          <textarea
            value={cvText}
            onChange={(e) => setCVText(e.target.value)}
            placeholder="Paste your CV here... (Include your name, experience, education, and skills)"
            className="w-full h-64 p-4 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
          />
          <button
            onClick={handlePaste}
            className="px-4 py-2 bg-gray-200 text-gray-800 rounded hover:bg-gray-300 transition-colors"
          >
            📋 Paste from Clipboard
          </button>
        </div>
      )}

      <button
        onClick={handleSubmit}
        disabled={loading}
        className="w-full px-4 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 transition-colors font-medium"
      >
        {loading ? 'Processing...' : '✨ Enhance My CV for This Job'}
      </button>
    </div>
  );
}
