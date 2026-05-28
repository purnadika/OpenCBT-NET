using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace OpenCBT.Tests.E2E.Pages;

public class AdminExamsPage : BasePage
{
    public AdminExamsPage(IWebDriver driver) : base(driver)
    {
    }

    public void CreateExam(string baseUrl, string title, string description, int durationMinutes, bool requireToken, bool randomize, string gradeName)
    {
        Driver.Navigate().GoToUrl(baseUrl.TrimEnd('/') + "/Admin/Exams/Create");
        
        Type(By.Id("Input_Title"), title);
        Type(By.Id("Input_Description"), description);
        
        var startTime = DateTime.UtcNow.AddMinutes(-10).ToString("yyyy-MM-ddTHH\\:mm");
        var endTime = DateTime.UtcNow.AddHours(2).ToString("yyyy-MM-ddTHH\\:mm");
        
        IJavaScriptExecutor js = (IJavaScriptExecutor)Driver;
        js.ExecuteScript($"var e = document.getElementById('Input_StartTime'); e.value = '{startTime}'; e.dispatchEvent(new Event('change')); e.dispatchEvent(new Event('input'));");
        js.ExecuteScript($"var e = document.getElementById('Input_EndTime'); e.value = '{endTime}'; e.dispatchEvent(new Event('change')); e.dispatchEvent(new Event('input'));");
        
        Type(By.Id("Input_DurationMinutes"), durationMinutes.ToString());

        var displayModeSelect = new SelectElement(Driver.FindElement(By.Id("Input_DisplayMode")));
        displayModeSelect.SelectByValue("1"); // SinglePage
        
        var gradeSelect = new SelectElement(Driver.FindElement(By.Id("Input_GradeId")));
        gradeSelect.SelectByText(gradeName);

        if (requireToken)
        {
            js.ExecuteScript("var chk = document.getElementById('Input_TokenRequired'); if(chk) { chk.checked = true; chk.value = 'true'; }");
            js.ExecuteScript("var hid = document.querySelector('input[type=\"hidden\"][name=\"Input.TokenRequired\"]'); if(hid) { hid.remove(); }");
        }

        if (randomize)
        {
            js.ExecuteScript("var chk = document.getElementById('Input_RandomizeQuestions'); if(chk) { chk.checked = true; chk.value = 'true'; }");
            js.ExecuteScript("var hid = document.querySelector('input[type=\"hidden\"][name=\"Input.RandomizeQuestions\"]'); if(hid) { hid.remove(); }");
        }

        Console.WriteLine($"TITLE BEFORE SUBMIT: {js.ExecuteScript("return document.getElementById('Input_Title').value;")}");

        var formDataJson = js.ExecuteScript("var form = document.querySelector('form'); var formData = new FormData(form); var obj = {}; formData.forEach((value, key) => obj[key] = value); return JSON.stringify(obj);");
        Console.WriteLine($"FORM DATA: {formDataJson}");

        Driver.FindElement(By.XPath("//button[contains(text(), 'Save Exam')]")).Click();
        
        // Wait for redirect to Exams Index
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
        try {
            wait.Until(d => d.Url.Contains("/Admin/Exams") && !d.Url.Contains("Create"));
        } catch (WebDriverTimeoutException) {
            // Log page source to see validation errors
            var src = Driver.PageSource;
            Console.WriteLine("PAGE SOURCE ON TIMEOUT:");
            Console.WriteLine(src);
            throw;
        }
    }
}
