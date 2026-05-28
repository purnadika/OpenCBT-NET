# OpenCBT .NET Modernization Release Notes

## [Enhancement & Bug Fixes] E2E Lifecycle Testing & Grade Targeting
*   **Grade and Class Room Targeting:** Introduced new `Grade` and `ClassRoom` entities. Exams are now tightly coupled to specific Grades.
*   **Token Generation Automation:** Fixed an issue where the `TokenRequired` toggle during exam creation did not automatically generate a token. The backend now instantly generates and attaches a 5-character token.
*   **Token Input Length Constraints:** Synchronized the token generation logic (5-characters) with the strict `<input maxlength="5">` UI constraints to prevent token validation failures caused by silent client-side truncation.
*   **E2E Integration Lifecycle:** Stabilized the full `ExecuteFullLifecycle` E2E xUnit tests by explicitly bypassing Javascript click-interceptions, utilizing isolated in-memory DB instances, and resolving dynamic XPath UI locators. The test suite is 100% green.

---

This release note highlights the modernization achievements and features added during **Phase 5** and **Phase 6**.

## [Modernization Feature] Phase 5: Excel Bulk Imports & Student Exam Cards

To achieve absolute administrative parity with the legacy PHP application, we introduced high-performance bulk data imports and printable exam tickets:

*   **Dedicated ClosedXML Templates**: Admins and teachers can now download separate, dedicated, and perfectly structured Excel templates for:
    *   **Student Bulk Imports** (Name, Identifier/NISN, Email, Password).
    *   **Multiple-Choice Questions** (Question text, points, options A-D, correct option letter).
    *   **Essay Questions** (Question text, points).
*   **Eager & Dynamic Import Engine**: Re-engineered the import parsing layer with a dynamic header matching algorithm, automatically mapping columns regardless of spreadsheet ordering or whether it is multiple-choice or essay.
*   **Printable Exam Card Tickets**: Designed a high-fidelity, printable (`@media print` optimized) HTML grid displaying:
    *   Student NISN/ID, login email, and seeded password.
    *   Dynamic proctor-assigned upcoming scheduled exam titles and dates.
    *   Decorative photo placeholders.
    *   A scan-to-login QR code dynamically fetched via standard API.

---

## [Modernization Feature] Phase 6: Teacher Analytics Dashboard & Essay Grading Panel

We completed the core modern computer-based examination loop by introducing comprehensive exam analytics and manual grading tools for teachers:

*   **Manual Essay Grading Workspace**: Built a premium, side-by-side grading workspace for teachers. Displays student written submissions alongside question prompts, allowing manual point entries and personalized feedback.
*   **Real-time Score Recalculation**: Submitting a manual essay grade triggers atomic calculations to update the `TotalScore` of the `ExamSession` instantly.
*   **Exam Analytics & Item Analysis**: Features an elegant dashboard with summary cards for total participants, completion rates, class average, high/low scores, and:
    *   A student performance roster displaying progress status and pending essay flags.
    *   An **Item Analysis (Question Accuracy)** grid indicating the success/pass rate of every question on the exam, assisting teachers in finding weak points.
*   **Modern UTC DateTime Mapping**: Removed legacy switches and configured EF Core global value converters to automatically force `DateTimeKind.Utc` on read and write, ensuring bulletproof multi-timezone execution in PostgreSQL.

---

## [Modernization Feature] Phase 7: Student Portal - Exam History, Graded Review & Performance Trends

We finalized the complete student lifecycle of the CBT application by designing a dedicated, highly personalized Student Portal Dashboard:

*   **Student Portal Dashboard**: Modernized the landing workspace displaying statistics cards for "Exams Completed", "Average Score", and "Active Exams". Separates active exams and past completed exams cleanly.
*   **Graded Review Workspace**: Students can now review any completed, fully-graded exam in a comprehensive review dashboard showing:
    *   Multiple-choice questions with color cues (green/red borders) for correct and incorrect selections.
    *   Essay questions showing their typed written answers, the points awarded by the teacher, and the teacher's grading feedback.
*   **Secure Student Scope**: Integrated robust security validation checking that students are strictly authorized to view only their own exam sessions, throwing `UnauthorizedAccessException` otherwise.

---

## [Modernization Feature] Phase 8: Printable Graded Report Cards & Teacher Roster Archives

We completed the visualization and archiving capabilities of the CBT application by developing high-fidelity browser-printable documents:

*   **Student Graded Report Card & Performance Certificate**: Created an elegant print-ready report card view `/Exams/ReportCard` which includes:
    *   Dynamic student details, exam name, and percentage calculations.
    *   A scorecard indicating passing classifications (requires >= 60%).
    *   A full question scorecard and a dedicated teacher comments box.
    *   Verifiable proctor signature lines and an online validation QR code.
*   **Teacher Roster Ledger Print View**: Developed a dedicated archival print layout `/Admin/Exams/PrintGrades` for teachers to print full class rosters, including averages and supervisor/headmaster signature boards for school archival filing.
*   **Print-Only CSS Optimizations**: Implemented precise `@media print` rules, guaranteeing perfect physical paper or PDF outputs on standard browsers without heavy server-side package dependencies.
