'use client';

interface JobDescriptionInputProps {
  value: string;
  onChange: (value: string) => void;
}

export default function JobDescriptionInput({
  value,
  onChange,
}: JobDescriptionInputProps) {
  const handlePaste = async () => {
    try {
      const text = await navigator.clipboard.readText();
      onChange(text);
    } catch (err) {
      console.error('Failed to read clipboard:', err);
    }
  };

  return (
    <div className="space-y-3">
      <textarea
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder="Paste the complete job description here... (Include title, description, requirements, and qualifications)"
        className="w-full h-64 p-4 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none font-sans"
      />
      <button
        onClick={handlePaste}
        className="px-4 py-2 bg-gray-200 text-gray-800 rounded hover:bg-gray-300 transition-colors"
      >
        📋 Paste from Clipboard
      </button>
      <p className="text-sm text-gray-600">
        Include job title, responsibilities, required skills, and qualifications for best results.
      </p>
    </div>
  );
}
