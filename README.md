# OpenCBT.NET - Modern Computer-Based Testing Portal

OpenCBT.NET is a modernized, high-performance, and scalable Computer-Based Testing (CBT) portal migrated from legacy PHP systems into a robust **ASP.NET Core 10 Web Application**. Built on **Clean N-Tier Architecture** and optimized for extreme concurrency, it incorporates real-time SignalR proctoring, eager database loaders, Redis caching, dynamic Alpine.js UI engines, and standard-compliant PostgreSQL multi-timezone structures.

---

## 🌟 Modern Key Features

### 1. High-Performance & Concurrency Scaling
*   **Clean N-Tier Architecture**: Strictly separates Domain, Application, Infrastructure, and Web presentation layers (0 circular dependencies).
*   **Database Pooling & Concurrency Controls**: Leverages EF Core DbContext pooling and optimistic concurrency (RowVersion tokens) to comfortably handle 1000+ simultaneous student connections.
*   **Stateless Distributed Cache**: Injects Redis as a distributed cache to maintain a completely stateless presenting server, ideal for cloud scale.
*   **Npgsql UTC DateTime Standard**: Configures EF Core global value converters to automatically force `DateTimeKind.Utc` on all DateTime properties, ensuring seamless execution across timezones without server failures.

### 2. Student Examination Portal
*   **Configurable Display Modes**: Supports switchable UI layouts configured via the teacher dashboard:
    *   **Wizard Mode**: One question at a time with instant blur-saving (powered by Alpine.js).
    *   **Single Page Mode**: Traditional long scrolling layout.
*   **AJAX Auto-Saving**: Automatically saves student multiple-choice selections and essay keystrokes asynchronously to prevent data loss.
*   **Proctor Chat Sticky Widget**: Embedded SignalR chat enabling students to talk to active proctors directly inside the exam viewport.

### 3. Real-Time Proctor Monitoring Dashboard
*   **SignalR Communication**: Active proctors monitor student progress (current question indices, active status) in real-time.
*   **Supervisor Commands**: Proctors can send global broadcast alerts, individual personal messages, or trigger **Force Submissions** to instantly lock cheating students out of the exam.

### 4. Excel Imports & Printable Exam Tickets
*   **ClosedXML Imports**: Bulk register students or import multiple-choice/essay questions directly from Excel spreadsheets using dynamic column-mapping algorithms.
*   **Printable Exam Card Tickets**: Generates printable (`@media print` optimized) cards complete with dynamic proctor-scheduled dates, student login credentials, and scan-to-login QR codes.

### 5. Manual Essay Evaluation Workspace
*   **Teacher Evaluation Area**: Dedicated grading dashboard to review student written essay answers side-by-side with questions. Teachers award points and write personalized feedback.
*   **Atomic Overall Recalculation**: Automatically updates the `TotalScore` of the `ExamSession` upon essay point submission.

### 6. Classroom Analytics & Item Analysis
*   **Exam Summary Statistics**: Cards displaying class average, highest score, lowest score, and completion progress metrics.
*   **Item Analysis (Question Accuracy)**: Statistical grids displaying the exact success/pass rate of every single question on the exam, helping teachers identify problematic concepts.

### 7. Printable Graded Report Cards & Ledger Archives
*   **Report Cards & Performance Certificates**: Beautiful print layouts showing total scores, percentage grades, PASS/FAIL status, individual scoring details, supervisor signature lines, and validation QR codes.
*   **Teacher Archival Ledger**: Printable class grade ledger for headmaster approvals and school archives.

---

## 🚀 Quick Start (Docker)

Spin up the entire stack (ASP.NET Core Web App, PostgreSQL, Redis) with a single command:
```powershell
docker-compose up --build
```
*   **Web Portal URL**: `http://localhost:8080`
*   **PostgreSQL External Host Port**: `5555`
*   **Default Admin Account**: `admin@opencbt.local` (Password: `Admin123!`)
*   **Default Student Account**: `student@opencbt.local` (Password: `Student123!`)

---

## 🧪 Running Unit Tests

The solution features a robust, mock-configured unit test suite. Run them locally:
```powershell
dotnet test --filter FullyQualifiedName~OpenCBT.Tests.Unit
```