using Microsoft.AspNetCore.Mvc.Testing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using OpenCBT.Tests.E2E.Core;
using OpenCBT.Tests.E2E.Pages;
using Microsoft.Extensions.DependencyInjection;
using OpenCBT.Infrastructure.Data;

namespace OpenCBT.Tests.E2E;

public class OpenCBTTest : IClassFixture<OpenCBTWebApplicationFactory>, IDisposable
{
    private readonly OpenCBTWebApplicationFactory _factory;
    private readonly IWebDriver _driver;
    private readonly string _baseUrl;

    public OpenCBTTest(OpenCBTWebApplicationFactory factory)
    {
        _factory = factory;
        _baseUrl = _factory.ServerAddress;
        
        var options = new ChromeOptions();
        // options.AddArgument("--headless=new"); // Disabled so user can see it running
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");
        _driver = new ChromeDriver(options);
    }

    [Fact]
    public void ExecuteFullLifecycle()
    {
        string token = "TKN99";
        Guid examId = Guid.NewGuid();

        // Setup database cleanly for this test and seed exam data
        using (var scope = _factory.KestrelServices.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OpenCBT.Infrastructure.Data.ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<OpenCBT.Domain.Entities.ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<OpenCBT.Domain.Entities.ApplicationRole>>();
            
            db.Database.EnsureCreated();
            OpenCBT.Infrastructure.Data.DbSeeder.SeedAsync(db, userManager, roleManager).GetAwaiter().GetResult();
        }

        // Phase 1: Admin Infrastructure Setup
        var adminLogin = new AdminLoginPage(_driver);
        adminLogin.GoTo(_baseUrl);
        var dashboard = adminLogin.Login(_baseUrl, "admin@opencbt.local", "Admin123!");
        
        WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));

        // Phase 1.5: Staff Management (Create Teacher, Force Password Reset)
        _driver.Navigate().GoToUrl(_baseUrl.TrimEnd('/') + "/Admin/Staff");
        wait.Until(d => d.FindElement(By.Id("CreateStaffBtn"))).Click();
        wait.Until(d => d.FindElement(By.Id("Input_FullName"))).SendKeys("E2E Teacher");
        _driver.FindElement(By.Id("Input_Email")).SendKeys("teacher_e2e@opencbt.local");
        _driver.FindElement(By.Id("Input_IdentifierNumber")).SendKeys("NIP123");
        // Role is Teacher by default, MustChangePassword is true by default
        _driver.FindElement(By.XPath("//button[contains(text(), 'Create Staff')]")).Click();
        
        // Wait for success message to extract auto-generated password
        var successMsg = wait.Until(d => d.FindElement(By.Id("SuccessMessageAlert"))).Text;
        var tempPassword = successMsg.Split("Auto-generated password: ")[1].Trim();
        
        // Logout Admin
        _driver.Navigate().GoToUrl(_baseUrl.TrimEnd('/') + "/Account/Logout");
        
        // Login as Teacher using temp password
        adminLogin.GoTo(_baseUrl);
        _driver.FindElement(By.Id("Input_Email")).SendKeys("teacher_e2e@opencbt.local");
        _driver.FindElement(By.Id("Input_Password")).SendKeys(tempPassword);
        _driver.FindElement(By.XPath("//button[@type='submit']")).Click();
        
        // Ensure we are redirected to ForceChangePassword
        wait.Until(d => d.Url.Contains("ForceChangePassword"));
        _driver.FindElement(By.Id("Input_NewPassword")).SendKeys("Teacher123!");
        _driver.FindElement(By.Id("Input_ConfirmPassword")).SendKeys("Teacher123!");
        _driver.FindElement(By.XPath("//button[@type='submit']")).Click();
        
        // Ensure successful login as teacher (redirected to Admin dashboard)
        wait.Until(d => d.Url.EndsWith("/Admin"));

        // Logout Teacher and login back as Admin to continue the rest of the E2E flow
        _driver.Navigate().GoToUrl(_baseUrl.TrimEnd('/') + "/Account/Logout");
        adminLogin.GoTo(_baseUrl);
        dashboard = adminLogin.Login(_baseUrl, "admin@opencbt.local", "Admin123!");

        // Create Grade
        _driver.Navigate().GoToUrl(_baseUrl.TrimEnd('/') + "/Admin/Grades");
        wait.Until(d => d.FindElement(By.Id("GradeNameInput"))).SendKeys("X MIPA");
        _driver.FindElement(By.Id("CreateGradeBtn")).Click();
        wait.Until(d => d.FindElement(By.XPath("//td[contains(text(), 'X MIPA')]")));

        // Create Class
        _driver.Navigate().GoToUrl(_baseUrl.TrimEnd('/') + "/Admin/Classes");
        wait.Until(d => d.FindElement(By.Id("ClassNameInput"))).SendKeys("MIPA 1");
        _driver.FindElement(By.Id("CreateClassBtn")).Click();
        wait.Until(d => d.FindElement(By.XPath("//td[contains(text(), 'MIPA 1')]")));

        var studentsPage = dashboard.GoToStudents();

        // 2. Create an Exam
        var examsPage = new AdminExamsPage(_driver);
        examsPage.CreateExam(_baseUrl, "Math Final Exam", "Semester 1 Final", 60, true, false, "X MIPA");

        // Get Exam ID by clicking on Questions button for the newly created Exam
        // Find the row containing "Math Final Exam" and click its Questions button
        var row = _driver.FindElement(By.XPath("//li[.//p[contains(text(), 'Math Final Exam')]]"));
        row.FindElement(By.XPath(".//a[contains(@href, '/Admin/Questions?examId=')]")).Click();
        var currentUrl = _driver.Url;
        examId = Guid.Parse(currentUrl.Split("examId=")[1]);

        // Add 15 MCQs and 5 Essays
        var questionsPage = new AdminQuestionsPage(_driver);
        for (int i = 1; i <= 15; i++)
        {
            questionsPage.AddMultipleChoiceQuestion($"Multiple Choice {i}", 5, new string[] { "Option A", "Option B", "Option C", "Option D", "Option E" }, 0);
        }
        for (int i = 16; i <= 20; i++)
        {
            questionsPage.AddEssayQuestion($"Essay {i}", 10);
        }

        // We must retrieve the token since we created it via UI which auto-generates a token!
        // We can get the token from the Database because the UI generates it and we need it for Student Login.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OpenCBT.Infrastructure.Data.ApplicationDbContext>();
            var uiExam = db.Exams.First(e => e.Title == "Math Final Exam");
            token = uiExam.CurrentToken;
        }

        dashboard.GoToStudents();

        string nisn1 = "111222333";
        string nisn2 = "444555666";

        studentsPage.RegisterStudent("Student One", "student1@test.com", nisn1, "Op3nCBT_Str0ngP@ssw0rd!2026");
        
        wait.Until(d => d.FindElement(By.XPath($"//td[contains(text(), '{nisn1}')]")));
        System.Threading.Thread.Sleep(1000); // Wait for modal to fade out

        studentsPage.RegisterStudent("Student Two", "student2@test.com", nisn2, "Op3nCBT_Str0ngP@ssw0rd!2026");
        try {
            wait.Until(d => d.FindElement(By.XPath($"//td[contains(text(), '{nisn2}')]")));
        } catch (Exception ex) {
            System.IO.File.WriteAllText("page_source_dump.html", _driver.PageSource);
            throw;
        }
        System.Threading.Thread.Sleep(1000); // Wait for modal to fade out

        // Phase 2: Student 1 Execution (Credential Login)
        _driver.Navigate().GoToUrl(_baseUrl.TrimEnd('/') + "/Account/Logout");
        adminLogin.GoTo(_baseUrl); // Wait for login page
        adminLogin.LoginAsStudent("student1@test.com", "Op3nCBT_Str0ngP@ssw0rd!2026");
        
        _driver.Navigate().GoToUrl(_baseUrl.TrimEnd('/') + $"/Exams/Take/{examId}");
        
        // Enter token
        try
        {
            wait.Until(d => d.FindElement(By.Name("token"))).SendKeys(token);
        }
        catch (WebDriverTimeoutException)
        {
            Console.WriteLine("=== PAGE SOURCE ON TIMEOUT ===");
            Console.WriteLine(_driver.PageSource);
            throw;
        }
        _driver.FindElement(By.XPath("//button[contains(text(), 'Enter Exam')]")).Click();
        
        // Wait for exam to load
        try
        {
            wait.Until(d => d.FindElement(By.XPath("//h3[contains(text(), 'Math Final Exam')]")));
        }
        catch (WebDriverTimeoutException)
        {
            Console.WriteLine("=== EXAM PAGE SOURCE ON TIMEOUT ===");
            Console.WriteLine(_driver.PageSource);
            throw;
        }

        // Select options
        var radios = _driver.FindElements(By.CssSelector("input[type='radio']"));
        if (radios.Count > 0) radios[0].Click();

        var textareas = _driver.FindElements(By.TagName("textarea"));
        if (textareas.Count > 0) textareas[0].SendKeys("Essay answer");

        // Submit exam
        _driver.FindElement(By.XPath("//button[contains(text(), 'Finish & Submit Exam')]")).Click();

        // Wait for redirect to Result
        wait.Until(d => d.Url.Contains("/Exams/Result"));

        // Phase 3: Student 2 Execution (Fast Pass Login)
        _driver.Navigate().GoToUrl(_baseUrl.TrimEnd('/') + "/Account/Logout");
        
        _driver.Navigate().GoToUrl(_baseUrl.TrimEnd('/') + "/Account/FastPass");
        wait.Until(d => d.FindElement(By.Id("Input_IdentifierNumber"))).SendKeys(nisn2);
        _driver.FindElement(By.Id("Input_ExamToken")).SendKeys(token);
        _driver.FindElement(By.XPath("//button[contains(text(), 'Start Exam')]")).Click();

        // Fast Pass logs us in and redirects to the Exam Take gateway
        wait.Until(d => d.FindElement(By.Name("token"))).SendKeys(token);
        _driver.FindElement(By.XPath("//button[contains(text(), 'Enter Exam')]")).Click();

        // Wait for exam to load
        wait.Until(d => d.FindElement(By.XPath("//h3[contains(text(), 'Math Final Exam')]")));
        
        var radios2 = _driver.FindElements(By.CssSelector("input[type='radio']"));
        if (radios2.Count > 0) radios2[1].Click();

        _driver.FindElement(By.XPath("//button[contains(text(), 'Finish & Submit Exam')]")).Click();
        wait.Until(d => d.Url.Contains("/Exams/Result"));

        // Phase 4: Admin Evaluation
        _driver.Navigate().GoToUrl(_baseUrl.TrimEnd('/') + "/Account/Logout");
        adminLogin.GoTo(_baseUrl);
        adminLogin.Login(_baseUrl, "admin@opencbt.local", "Admin123!");
        
        _driver.Navigate().GoToUrl(_baseUrl.TrimEnd('/') + $"/Admin/Exams/Analytics/{examId}");
        wait.Until(d => d.FindElement(By.TagName("body")));
        
        Assert.Contains("Student One", _driver.PageSource);
        Assert.Contains("Student Two", _driver.PageSource);
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}
