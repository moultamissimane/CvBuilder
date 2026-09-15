'use client';

import { useEffect, useState } from 'react';
import { api, AnalyzeResponse, GenerateTailoredCvResponse, StructuredCv, SavedCvSummary } from '@/lib/api';
import JobDescriptionInput from './JobDescriptionInput';
import CVInput from './CVInput';
import MatchAnalysisPanel from './MatchAnalysisPanel';
import TailoredCvEditor from './TailoredCvEditor';

type Step = 'input' | 'analysis' | 'editor';

export default function TailoredCvGenerator() {
  const [step, setStep] = useState<Step>('input');
  const [jobDescription, setJobDescription] = useState('');
  const [error, setError] = useState('');

  const [analyzing, setAnalyzing] = useState(false);
  const [analysis, setAnalysis] = useState<AnalyzeResponse | null>(null);

  const [generating, setGenerating] = useState(false);
  const [tailored, setTailored] = useState<GenerateTailoredCvResponse | null>(null);
  const [editableCv, setEditableCv] = useState<StructuredCv | null>(null);

  const [exporting, setExporting] = useState(false);
  const [saving, setSaving] = useState(false);
  const [saveMessage, setSaveMessage] = useState('');
  const [savedCvs, setSavedCvs] = useState<SavedCvSummary[]>([]);

  useEffect(() => {
    if (step === 'editor') {
      api.listSavedCvs().then(setSavedCvs).catch(() => {});
    }
  }, [step]);

  const handleAnalyze = async (cvInput: string | File, inputType: 'pdf' | 'text') => {
    if (!jobDescription.trim()) {
      setError('Please paste a job description first.');
      return;
    }

    setAnalyzing(true);
    setError('');

    try {
      const result =
        inputType === 'pdf'
          ? await api.analyzeTailoredCvFromPDF(cvInput as File, jobDescription)
          : await api.analyzeTailoredCvFromText(cvInput as string, jobDescription);

      setAnalysis(result);
      setStep('analysis');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred while analyzing your CV.');
    } finally {
      setAnalyzing(false);
    }
  };

  const handleGenerate = async () => {
    if (!analysis) return;

    setGenerating(true);
    setError('');

    try {
      const result = await api.generateTailoredCvDocument(analysis.cv, analysis.job);
      setTailored(result);
      setEditableCv(result.tailoredCv);
      setStep('editor');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An error occurred while generating the tailored CV.');
    } finally {
      setGenerating(false);
    }
  };

  const handleExportPdf = async () => {
    if (!editableCv) return;

    setExporting(true);
    setError('');

    try {
      const blob = await api.exportTailoredCvPdf(editableCv);
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${(editableCv.personalInfo.fullName || 'tailored-cv').replace(/\s+/g, '-').toLowerCase()}.pdf`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to export the CV as PDF.');
    } finally {
      setExporting(false);
    }
  };

  const handleSave = async () => {
    if (!editableCv || !analysis) return;

    setSaving(true);
    setError('');
    setSaveMessage('');

    try {
      const title = `${editableCv.personalInfo.fullName} - ${analysis.job.jobTitle || 'Tailored CV'}`.trim();
      const saved = await api.saveTailoredCv(
        editableCv,
        title,
        analysis.job.jobTitle,
        analysis.job.company,
        tailored?.analysis.matchScore ?? analysis.analysis.matchScore
      );
      setSavedCvs((prev) => [saved, ...prev]);
      setSaveMessage(`Saved as "${saved.title}"`);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save the CV.');
    } finally {
      setSaving(false);
    }
  };

  const handleLoadSaved = async (id: string) => {
    setError('');
    try {
      const detail = await api.getSavedCv(id);
      setEditableCv(detail.cv);
      setStep('editor');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load that saved CV.');
    }
  };

  const startOver = () => {
    setStep('input');
    setAnalysis(null);
    setTailored(null);
    setEditableCv(null);
    setSaveMessage('');
    setError('');
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 py-8">
      <div className="container mx-auto px-4">
        <div className="mb-8 text-center">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">Tailored CV Generator</h1>
          <p className="text-gray-600">
            Your original CV, reshaped for one job — nothing invented, nothing overwritten.
          </p>
        </div>

        <div className="mb-6 flex justify-center gap-2 text-sm">
          <StepPill label="1. CV & Job" active={step === 'input'} done={step !== 'input'} />
          <StepPill label="2. Match Analysis" active={step === 'analysis'} done={step === 'editor'} />
          <StepPill label="3. Preview & Export" active={step === 'editor'} done={false} />
        </div>

        {error && (
          <div className="mb-6 p-4 bg-red-100 border border-red-400 text-red-700 rounded-lg max-w-3xl mx-auto">
            {error}
          </div>
        )}

        {step === 'input' && (
          <div className="bg-white rounded-lg shadow-lg p-6 md:p-8 max-w-3xl mx-auto space-y-8">
            <div>
              <h2 className="text-2xl font-semibold text-gray-900 mb-4">Step 1: Paste the Job Description</h2>
              <JobDescriptionInput value={jobDescription} onChange={setJobDescription} />
            </div>
            <div className="border-t border-gray-300 pt-8">
              <h2 className="text-2xl font-semibold text-gray-900 mb-4">Step 2: Your Original CV</h2>
              <CVInput onCVSubmit={handleAnalyze} loading={analyzing} />
            </div>
          </div>
        )}

        {step === 'analysis' && analysis && (
          <div className="max-w-3xl mx-auto space-y-6">
            <div className="bg-white rounded-lg shadow-lg p-6 md:p-8">
              <MatchAnalysisPanel analysis={analysis.analysis} job={analysis.job} />
            </div>
            <div className="flex gap-3">
              <button
                onClick={startOver}
                className="px-4 py-3 bg-gray-200 text-gray-800 rounded-lg hover:bg-gray-300 transition-colors font-medium"
              >
                ← Start Over
              </button>
              <button
                onClick={handleGenerate}
                disabled={generating}
                className="flex-1 px-4 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 disabled:bg-gray-400 transition-colors font-medium"
              >
                {generating ? 'Generating...' : '✨ Generate Tailored CV'}
              </button>
            </div>
          </div>
        )}

        {step === 'editor' && editableCv && (
          <div className="max-w-3xl mx-auto space-y-6">
            {tailored && tailored.validationIssues.length > 0 && (
              <div className="p-4 bg-red-100 border border-red-400 text-red-700 rounded-lg text-sm">
                <p className="font-semibold mb-1">This version needs review before you send it:</p>
                <ul className="list-disc list-inside">
                  {tailored.validationIssues.map((issue, i) => (
                    <li key={i}>{issue}</li>
                  ))}
                </ul>
              </div>
            )}

            {saveMessage && (
              <div className="p-3 bg-green-100 border border-green-400 text-green-700 rounded-lg text-sm">
                {saveMessage}
              </div>
            )}

            <div className="flex justify-between items-center">
              <button onClick={startOver} className="text-sm text-gray-600 hover:text-gray-900">
                ← Start a new tailored CV
              </button>
            </div>

            <TailoredCvEditor
              cv={editableCv}
              onChange={setEditableCv}
              onExportPdf={handleExportPdf}
              onSave={handleSave}
              exporting={exporting}
              saving={saving}
            />

            {savedCvs.length > 0 && (
              <div className="bg-white rounded-lg shadow p-4">
                <h3 className="font-semibold text-gray-900 mb-2 text-sm">Your Saved Versions</h3>
                <div className="space-y-1">
                  {savedCvs.map((s) => (
                    <button
                      key={s.id}
                      onClick={() => handleLoadSaved(s.id)}
                      className="w-full text-left px-3 py-2 rounded hover:bg-gray-100 text-sm flex justify-between items-center"
                    >
                      <span>
                        {s.title} {s.company && <span className="text-gray-500">· {s.company}</span>}
                      </span>
                      <span className="text-gray-400 text-xs">{Math.round(s.matchScore)}%</span>
                    </button>
                  ))}
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}

function StepPill({ label, active, done }: { label: string; active: boolean; done: boolean }) {
  return (
    <span
      className={`px-3 py-1 rounded-full font-medium ${
        active
          ? 'bg-blue-600 text-white'
          : done
          ? 'bg-blue-100 text-blue-700'
          : 'bg-gray-100 text-gray-500'
      }`}
    >
      {label}
    </span>
  );
}
