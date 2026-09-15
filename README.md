# CV Builder - Job Description Based Resume Tailoring

A modern web application that helps users create and tailor their CVs based on specific job descriptions. Built with **Next.js** (frontend) and **.NET 6** (backend).

## 🌟 Features

- **Job Description Analysis**: Paste a job description and the AI analyzes it to extract required skills and qualifications
- **CV Tailoring**: Automatically tailors your CV summary and highlights relevant skills for the job
- **Match Scoring**: Calculates a compatibility score between your CV and the job description
- **Multiple Templates**: Choose from different CV templates (Modern, Classic, Minimalist, Creative)
- **PDF Export**: Print or save your tailored CV as a PDF
- **Recommendations**: Get AI-powered suggestions to improve your match score
- **User Accounts**: Save multiple CVs and job applications (future enhancement)
- **Responsive Design**: Works seamlessly on desktop and mobile devices

## 🏗️ Project Structure

```
cv-builder/
├── frontend/                 # Next.js application
│   ├── src/
│   │   ├── app/            # App Router pages
│   │   ├── components/     # React components
│   │   ├── lib/            # Utilities and API client
│   │   └── styles/         # CSS/Tailwind styles
│   ├── package.json
│   └── .env.local          # Environment variables
├── backend/                 # .NET Web API
│   ├── Controllers/        # API endpoints
│   ├── Models/            # Data models and DTOs
│   ├── Services/          # Business logic
│   └── Program.cs         # Application startup
└── README.md
```

## 🚀 Getting Started

### Prerequisites

- **Frontend**: Node.js 18+ and npm/yarn
- **Backend**: .NET 6 SDK
- **Database**: Not required for MVP (in-memory storage)

### Backend Setup

```bash
cd backend

# Restore dependencies
dotnet restore

# Run the API (default port: 5000)
dotnet run

# The API will be available at https://localhost:5001 and http://localhost:5000
```

API endpoints:
- `POST /api/cv/generate` - Generate tailored CV
- `POST /api/cv/analyze` - Analyze job description
- `GET /api/cv/templates` - Get available templates
- `POST /api/cv/match-score` - Calculate match score

### Frontend Setup

```bash
cd frontend

# Install dependencies
npm install

# Set environment variables (already in .env.local)
# NEXT_PUBLIC_API_URL=http://localhost:5000/api

# Run development server (default port: 3000)
npm run dev
```

Visit `http://localhost:3000` in your browser.

## 📝 How to Use

1. **Paste Job Description**
   - Navigate to the "Job Description" tab
   - Paste the complete job description
   - Click "Generate Tailored CV"

2. **Fill Your Information**
   - Go to the "Your CV" tab
   - Enter your personal information, experiences, education, and skills
   - Fill in at least name and email

3. **View Results**
   - Go to the "Preview" tab
   - See your tailored CV with AI suggestions
   - View match score, recommended skills, and improvements

4. **Export CV**
   - Click "Print / Save as PDF"
   - Choose your PDF printer or save to file

## 🤖 AI Algorithm

The CV Builder uses a custom keyword-matching algorithm (not yet integrated with external AI APIs):

### Match Score Calculation
- **Skill Matching** (40%): Percentage of job requirements matched in your skills
- **Experience Relevance** (30%): How recent and relevant your work experience is
- **Education** (20%): Presence and relevance of education credentials
- **Summary Quality** (10%): Completeness of your professional summary

### Key Skill Extraction
- Regex patterns to identify skills from job descriptions
- Support for common skill keywords and technologies
- Deduplication and ranking by frequency

### Recommendations
- Suggests missing skills to add
- Identifies most relevant work experiences
- Recommends filling in missing CV sections

## 🔧 Future Enhancements

- [ ] User authentication and CV storage
- [ ] Integration with OpenAI/Claude for better AI analysis
- [ ] PDF generation and download
- [ ] Job application history tracking
- [ ] Email reminders for saved jobs
- [ ] Cover letter generation
- [ ] Portfolio links integration
- [ ] LinkedIn import/export
- [ ] Analytics dashboard for job seekers
- [ ] Batch job application analysis

## 🛠️ Technology Stack

### Frontend
- **Next.js 14** - React framework with App Router
- **TypeScript** - Type-safe JavaScript
- **Tailwind CSS** - Utility-first CSS
- **React 18** - UI library

### Backend
- **.NET 6** - Web framework
- **ASP.NET Core** - API framework
- **C#** - Programming language

## 📋 API Request/Response Examples

### Generate Tailored CV

**Request:**
```json
POST /api/cv/generate
Content-Type: application/json

{
  "jobDescription": "We are looking for a Senior Software Engineer...",
  "userCV": "{\"fullName\": \"John Doe\", \"skills\": [\"C#\", \"JavaScript\"]...}"
}
```

**Response:**
```json
{
  "tailoredSummary": "Experienced software engineer with expertise in...",
  "keySkillsToHighlight": ["C#", "JavaScript", "React"],
  "relevantExperiences": ["Senior Developer at Tech Corp"],
  "matchScore": 75.5,
  "recommendations": ["Consider adding cloud technologies..."]
}
```

## 🧪 Development Tips

### Debugging
- Frontend: Open DevTools (F12) for console logs and network requests
- Backend: Check console output from `dotnet run`
- API: Visit `https://localhost:5001/swagger` for Swagger documentation

### Running Tests (Future)
```bash
# Backend tests
cd backend
dotnet test

# Frontend tests
cd frontend
npm run test
```

## 📄 License

MIT License - feel free to use this project as a starting point!

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## ❓ FAQ

**Q: Can I use my existing CV?**
A: Currently, you need to manually enter your information. Future versions will support CV import.

**Q: Is my data saved?**
A: In the MVP, data is only stored in your browser session. Future versions will include secure cloud storage.

**Q: Can I generate multiple tailored CVs?**
A: Yes! Each time you paste a new job description, a new analysis is performed.

**Q: How accurate is the match score?**
A: The MVP uses keyword matching. Integration with advanced AI models will improve accuracy.

---

**Happy Job Hunting! 🚀**
# CvBuilder
