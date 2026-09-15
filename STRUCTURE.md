# Project Structure Overview

## CV Builder Project Structure

```
cv-builder/
│
├── 📁 frontend/                  # Next.js React Application
│   ├── 📁 src/
│   │   ├── 📁 app/
│   │   │   └── page.tsx          # Home page (Main entry point)
│   │   │
│   │   ├── 📁 components/        # React Components
│   │   │   ├── CVBuilderMain.tsx       # Main wrapper component with tabs
│   │   │   ├── JobDescriptionInput.tsx # Job description input section
│   │   │   ├── CVForm.tsx              # CV editing form
│   │   │   ├── CVPreview.tsx           # CV preview/export
│   │   │   └── GeneratedSuggestions.tsx # AI suggestions display
│   │   │
│   │   ├── 📁 lib/
│   │   │   └── api.ts            # API client and types
│   │   │
│   │   └── 📁 styles/            # Tailwind CSS (global styles)
│   │
│   ├── package.json              # Frontend dependencies
│   ├── tsconfig.json             # TypeScript configuration
│   ├── tailwind.config.js        # Tailwind CSS config
│   ├── next.config.js            # Next.js configuration
│   ├── .env.local                # Environment variables
│   └── .eslintrc.json            # ESLint configuration
│
├── 📁 backend/                   # .NET Core Web API
│   ├── 📁 Controllers/
│   │   └── CVController.cs       # CV API endpoints
│   │
│   ├── 📁 Models/
│   │   ├── JobDescription.cs     # Job description model
│   │   ├── CV.cs                 # CV and related models
│   │   │
│   │   └── 📁 DTOs/
│   │       └── CVDtos.cs         # Data transfer objects for API
│   │
│   ├── 📁 Services/
│   │   └── CVGenerationService.cs # Business logic for CV matching
│   │
│   ├── Program.cs                # Application startup and DI setup
│   ├── CVBuilder.API.csproj      # Backend project file
│   ├── Properties/
│   │   └── launchSettings.json   # Debug launch settings
│   │
│   └── appsettings.json          # Configuration settings
│
├── 📄 README.md                  # Main project documentation
├── 📄 QUICKSTART.md              # Quick start guide
├── 📄 docker-compose.yml         # Docker composition for local dev
└── 📄 .gitignore                 # Git ignore rules
```

## Key Features Location

| Feature | Location | Description |
|---------|----------|-------------|
| **Job Analysis** | `backend/Services/CVGenerationService.cs` | Extracts skills from job descriptions |
| **Match Scoring** | `backend/Services/CVGenerationService.cs` | Calculates CV-to-job fit (40% skills, 30% exp, 20% edu, 10% summary) |
| **CV Tailoring** | `backend/Services/CVGenerationService.cs` | Generates tailored summary and recommendations |
| **API Client** | `frontend/src/lib/api.ts` | Communicates with .NET backend |
| **UI/Components** | `frontend/src/components/` | React components for UI |
| **API Endpoints** | `backend/Controllers/CVController.cs` | RESTful API endpoints |
| **Data Models** | `backend/Models/` | C# classes for CV and Job data |

## Architecture

### Frontend Flow
```
User Input
    ↓
CVBuilderMain (State Management)
    ├→ JobDescriptionInput (Tab 1)
    ├→ CVForm (Tab 2)
    └→ CVPreview + GeneratedSuggestions (Tab 3)
    ↓
API Client (lib/api.ts)
    ↓
.NET Backend
```

### Backend Flow
```
HTTP Request
    ↓
CVController
    ↓
CVGenerationService
    ├→ ExtractKeywordsFromJobDescription()
    ├→ CalculateMatchScore()
    ├→ GenerateTailoredSummary()
    └→ RecommendSkillsToAdd()
    ↓
HTTP Response (JSON)
```

## File Dependencies

```
Frontend:
- page.tsx imports CVBuilderMain
- CVBuilderMain imports all component files
- Components import from lib/api.ts
- api.ts defines types and API calls

Backend:
- Program.cs registers CVGenerationService
- CVController injects ICVGenerationService
- Services use Models and DTOs
- DTOs map between API and business logic
```

## Development Workflow

1. **Start Backend**: `cd backend && dotnet run` (Port 5000)
2. **Start Frontend**: `cd frontend && npm run dev` (Port 3000)
3. **Modify Components**: Edit files in `frontend/src/components/`
4. **Modify API Logic**: Edit `backend/Services/CVGenerationService.cs`
5. **Add API Endpoints**: Add methods to `backend/Controllers/CVController.cs`
6. **Add Models**: Create new files in `backend/Models/`

## Configuration Files

| File | Purpose | Location |
|------|---------|----------|
| `next.config.js` | Next.js build config | `frontend/` |
| `tailwind.config.js` | Tailwind CSS setup | `frontend/` |
| `tsconfig.json` | TypeScript config | `frontend/` |
| `.env.local` | Environment variables | `frontend/` |
| `Program.cs` | .NET startup | `backend/` |
| `appsettings.json` | App settings | `backend/` |
| `CVBuilder.API.csproj` | Project dependencies | `backend/` |
| `docker-compose.yml` | Docker setup | Root |

## Component Communication

```typescript
Frontend Component → api.ts → HTTP POST → CVController
                                             ↓
                                       CVGenerationService
                                             ↓
                                         Response JSON
                                             ↓
Frontend Component ← Displays Results ← api.ts
```

## Database (Future)

When adding database support:
```
Backend:
├── 📁 Data/
│   └── ApplicationDbContext.cs   # EF Core context
├── 📁 Migrations/                # EF Core migrations
└── 📁 Repository/                # Data access layer
```

Add to appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=CVBuilderDB;..."
  }
}
```

---

Last Updated: 2024
