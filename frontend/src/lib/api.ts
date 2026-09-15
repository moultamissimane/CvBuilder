const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api';

export interface JobDescriptionAnalysis {
  extractedSkills: string[];
  skillCount: number;
  analysisDate: string;
}

export interface CVGenerationResponse {
  tailoredSummary: string;
  keySkillsToHighlight: string[];
  relevantExperiences: string[];
  matchScore: number;
  recommendations: string[];
}

export interface SkillAddition {
  skill: string;
  level: string;
  isAdded?: boolean;
}

export interface SyntheticExperienceAddition {
  title: string;
  description: string;
  skillsUsed: string[];
  isProposed: boolean;
}

export interface EnhancedCVResponse {
  originalCV: string;
  fullName: string;
  title: string;
  contactLine: string;
  email: string;
  phone: string;
  summary: string;
  experienceText: string;
  educationText: string;
  certificationsText: string;
  skillsText: string;
  languagesText: string;
  addedSkills: SkillAddition[];
  syntheticExperience: SyntheticExperienceAddition;
  matchScore: number;
  recommendations: string[];
}

export interface CVData {
  title: string;
  fullName: string;
  email: string;
  phone: string;
  summary: string;
  experiences: Experience[];
  education: Education[];
  skills: string[];
  templateId?: string;
}

export interface Experience {
  companyName: string;
  jobTitle: string;
  startDate: string;
  endDate?: string;
  currentlyWorking: boolean;
  description: string;
  achievements: string[];
}

export interface Education {
  institution: string;
  degree: string;
  fieldOfStudy: string;
  graduationYear: number;
  grade?: string;
}

// ---- Tailored CV Generator ----

export interface PersonalInfo {
  fullName: string;
  title: string;
  email: string;
  phone: string;
  contactLine: string;
}

export interface StructuredExperience {
  jobTitle: string;
  company: string;
  dateRange: string;
  description: string[];
  technologies: string[];
  relevanceScore: number;
}

export interface StructuredEducation {
  degree: string;
  institution: string;
  dateRange: string;
  details: string[];
}

export interface StructuredProject {
  name: string;
  description: string;
  technologies: string[];
}

export interface SkillGroups {
  frontend: string[];
  backend: string[];
  databases: string[];
  cloud: string[];
  devOps: string[];
  tools: string[];
  other: string[];
}

export interface StructuredCv {
  personalInfo: PersonalInfo;
  summary: string;
  experience: StructuredExperience[];
  education: StructuredEducation[];
  skills: SkillGroups;
  certifications: string[];
  projects: StructuredProject[];
  languages: string[];
  rawExperienceText: string;
}

export interface StructuredJobDescription {
  jobTitle: string;
  company: string;
  requiredSkills: string[];
  preferredSkills: string[];
  responsibilities: string[];
  keywords: string[];
  experienceRequirement: string;
  rawText: string;
}

export type SkillMatchStatus = 'Verified' | 'Related' | 'NotVerified';

export interface SkillMatch {
  skill: string;
  status: SkillMatchStatus;
  isPreferred: boolean;
  evidence: string;
}

export interface MatchAnalysis {
  skillMatches: SkillMatch[];
  matchScore: number;
  requiredSkillCount: number;
  verifiedCount: number;
  relatedCount: number;
  notVerifiedCount: number;
}

export interface AnalyzeResponse {
  cv: StructuredCv;
  job: StructuredJobDescription;
  analysis: MatchAnalysis;
}

export interface GenerateTailoredCvResponse {
  tailoredCv: StructuredCv;
  analysis: MatchAnalysis;
  validationIssues: string[];
}

export interface SavedCvSummary {
  id: string;
  title: string;
  jobTitle: string;
  company: string;
  matchScore: number;
  createdAt: string;
}

export interface SavedCvDetail extends SavedCvSummary {
  cv: StructuredCv;
}

async function parseErrorMessage(response: Response, fallback: string): Promise<string> {
  try {
    const text = await response.text();
    return text || fallback;
  } catch {
    return fallback;
  }
}

class CVBuilderAPI {
  async enhanceCVFromPDF(file: File, jobDescription: string): Promise<EnhancedCVResponse> {
    const formData = new FormData();
    formData.append('CVFile', file);
    formData.append('JobDescription', jobDescription);

    const response = await fetch(`${API_BASE_URL}/cv/enhance-from-pdf`, {
      method: 'POST',
      body: formData,
    });

    if (!response.ok) {
      throw new Error('Failed to enhance CV from PDF');
    }

    return response.json();
  }

  async enhanceCVFromText(cvText: string, jobDescription: string): Promise<EnhancedCVResponse> {
    const response = await fetch(`${API_BASE_URL}/cv/enhance-from-text`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ cvText, jobDescription }),
    });

    if (!response.ok) {
      throw new Error('Failed to enhance CV from text');
    }

    return response.json();
  }

  async generateTailoredCV(jobDescription: string, userCV: CVData): Promise<CVGenerationResponse> {
    const response = await fetch(`${API_BASE_URL}/cv/generate`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        jobDescription,
        userCV: JSON.stringify(userCV),
      }),
    });

    if (!response.ok) {
      throw new Error('Failed to generate tailored CV');
    }

    return response.json();
  }

  async analyzeJobDescription(jobDescription: string): Promise<JobDescriptionAnalysis> {
    const response = await fetch(`${API_BASE_URL}/cv/analyze`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(jobDescription),
    });

    if (!response.ok) {
      throw new Error('Failed to analyze job description');
    }

    return response.json();
  }

  async getAvailableTemplates() {
    const response = await fetch(`${API_BASE_URL}/cv/templates`);

    if (!response.ok) {
      throw new Error('Failed to fetch templates');
    }

    return response.json();
  }

  async calculateMatchScore(cv: CVData, jobDescription: string): Promise<{ matchScore: number }> {
    const response = await fetch(`${API_BASE_URL}/cv/match-score`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ cv, jobDescription }),
    });

    if (!response.ok) {
      throw new Error('Failed to calculate match score');
    }

    return response.json();
  }

  async analyzeTailoredCvFromText(cvText: string, jobDescription: string): Promise<AnalyzeResponse> {
    const response = await fetch(`${API_BASE_URL}/tailored-cv/analyze-text`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ cvText, jobDescription }),
    });

    if (!response.ok) {
      throw new Error(await parseErrorMessage(response, 'Failed to analyze your CV.'));
    }

    return response.json();
  }

  async analyzeTailoredCvFromPDF(file: File, jobDescription: string): Promise<AnalyzeResponse> {
    const formData = new FormData();
    formData.append('CvFile', file);
    formData.append('JobDescription', jobDescription);

    const response = await fetch(`${API_BASE_URL}/tailored-cv/analyze-upload`, {
      method: 'POST',
      body: formData,
    });

    if (!response.ok) {
      throw new Error(await parseErrorMessage(response, 'Failed to analyze your CV.'));
    }

    return response.json();
  }

  async generateTailoredCvDocument(cv: StructuredCv, job: StructuredJobDescription): Promise<GenerateTailoredCvResponse> {
    const response = await fetch(`${API_BASE_URL}/tailored-cv/generate`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ cv, job }),
    });

    if (!response.ok) {
      throw new Error(await parseErrorMessage(response, 'Failed to generate the tailored CV.'));
    }

    return response.json();
  }

  async exportTailoredCvPdf(cv: StructuredCv): Promise<Blob> {
    const response = await fetch(`${API_BASE_URL}/tailored-cv/export-pdf`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ cv }),
    });

    if (!response.ok) {
      throw new Error(await parseErrorMessage(response, 'Failed to export the CV as PDF.'));
    }

    return response.blob();
  }

  async saveTailoredCv(cv: StructuredCv, title: string, jobTitle: string, company: string, matchScore: number): Promise<SavedCvSummary> {
    const response = await fetch(`${API_BASE_URL}/tailored-cv/save`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ cv, title, jobTitle, company, matchScore }),
    });

    if (!response.ok) {
      throw new Error(await parseErrorMessage(response, 'Failed to save the CV.'));
    }

    return response.json();
  }

  async listSavedCvs(): Promise<SavedCvSummary[]> {
    const response = await fetch(`${API_BASE_URL}/tailored-cv/saved`);

    if (!response.ok) {
      throw new Error('Failed to load saved CVs.');
    }

    return response.json();
  }

  async getSavedCv(id: string): Promise<SavedCvDetail> {
    const response = await fetch(`${API_BASE_URL}/tailored-cv/saved/${id}`);

    if (!response.ok) {
      throw new Error('Failed to load the saved CV.');
    }

    return response.json();
  }
}

export const api = new CVBuilderAPI();
