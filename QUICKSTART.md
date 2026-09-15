# CV Builder - Quick Start Guide

## 5-Minute Setup

### Option 1: Manual Setup (Recommended for Development)

#### Step 1: Start the Backend
```bash
cd backend
dotnet run
```
✅ Backend will be available at `http://localhost:5000`

#### Step 2: Start the Frontend (in a new terminal)
```bash
cd frontend
npm install
npm run dev
```
✅ Frontend will be available at `http://localhost:3000`

#### Step 3: Open in Browser
Navigate to `http://localhost:3000` and start building CVs!

---

### Option 2: Docker Setup (One Command)

```bash
docker-compose up
```

This will:
- Start the .NET backend on port 5000
- Start the Next.js frontend on port 3000
- Automatically install all dependencies

---

## 🧪 Testing the Application

### Sample Job Description
Copy and paste this into the Job Description field:

```
Senior Full Stack Developer

We are looking for an experienced Senior Full Stack Developer to join our growing team.

Requirements:
- 5+ years of software development experience
- Strong proficiency in JavaScript/TypeScript
- Experience with React or Vue.js
- Knowledge of Node.js or Python
- Database design and SQL experience
- Understanding of REST APIs and microservices
- Experience with Docker and Kubernetes
- Familiarity with CI/CD pipelines
- Git version control experience

Responsibilities:
- Design and implement scalable web applications
- Collaborate with product and design teams
- Write clean, maintainable code
- Conduct code reviews
- Mentor junior developers
- Participate in architectural decisions

Qualifications:
- Bachelor's degree in Computer Science or related field
- Strong problem-solving skills
- Excellent communication abilities
- Experience in Agile environments
```

### Sample User CV
Then fill in the "Your CV" tab with your information:
- Name: John Doe
- Email: john@example.com
- Skills: JavaScript, React, Node.js, Python, SQL, Docker
- Add a work experience and education entry

Then click "Generate Tailored CV" to see the results!

---

## 🔍 Checking API Endpoints

### Swagger Documentation
While the backend is running, visit:
```
https://localhost:5001/swagger
```

### Test API Manually
```bash
# Analyze a job description
curl -X POST http://localhost:5000/api/cv/analyze \
  -H "Content-Type: application/json" \
  -d '"Software engineer with skills in JavaScript"'

# Get available templates
curl http://localhost:5000/api/cv/templates
```

---

## ❓ Troubleshooting

### "Connection refused" error
- Make sure backend is running on port 5000
- Check that no other application is using port 5000
- Verify `.env.local` has correct API URL: `http://localhost:5000/api`

### "npm ERR! code ERESOLVE"
```bash
cd frontend
npm install --legacy-peer-deps
npm run dev
```

### ".NET SDK not found"
- Install .NET 6 SDK from: https://dotnet.microsoft.com/download/dotnet/6.0

### Frontend shows "Cannot connect to API"
- Check backend is running: `dotnet run` in backend directory
- Verify CORS is enabled in Program.cs
- Check browser console for actual error message

---

## 📚 Next Steps

1. **Add Authentication** - Use Auth0 or NextAuth.js
2. **Database Integration** - Add SQL Server or PostgreSQL
3. **Better AI** - Integrate OpenAI API for smarter CV generation
4. **User Accounts** - Save and manage multiple CVs
5. **PDF Generation** - Use a library like pdfkit or PrintRenderingService
6. **Deployment** - Deploy to Azure or AWS

---

## 📞 Support

- Check the main README.md for more information
- Review API endpoints in Controllers/CVController.cs
- Inspect browser DevTools (F12) for frontend errors
- Check console output in terminal for backend errors

Happy Building! 🚀
