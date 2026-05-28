using OpenQA.Selenium;

namespace OpenCBT.Tests.E2E.Pages;

public class AdminStudentsPage : BasePage
{
    public AdminStudentsPage(IWebDriver driver) : base(driver)
    {
        WaitAndFindElement(By.XPath("//h1[contains(text(), 'Manage Students')]"));
    }

    private By RegisterButton => By.XPath("//button[contains(text(), 'Register Student')]");
    
    // Modal Inputs
    private By FullNameInput => By.Id("NewStudent_FullName");
    private By EmailInput => By.Id("NewStudent_Email");
    private By NISNInput => By.Id("NewStudent_IdentifierNumber");
    private By PasswordInput => By.Id("NewStudent_Password");
    private By SubmitRegisterButton => By.XPath("//button[@type='submit' and contains(text(), 'Register')]");

    public void RegisterStudent(string fullName, string email, string nisn, string password)
    {
        System.Threading.Thread.Sleep(1000);
        var btn = WaitAndFindElement(RegisterButton);
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", btn);
        System.Threading.Thread.Sleep(1000); // Wait for modal animation
        Type(FullNameInput, fullName);
        Type(EmailInput, email);
        Type(NISNInput, nisn);
        Type(PasswordInput, password);
        
        // Select first grade
        var gradeSelect = new OpenQA.Selenium.Support.UI.SelectElement(WaitAndFindElement(By.Id("NewStudent_GradeId")));
        gradeSelect.SelectByIndex(1);
        
        // Select first class
        var classSelect = new OpenQA.Selenium.Support.UI.SelectElement(WaitAndFindElement(By.Id("NewStudent_ClassRoomId")));
        classSelect.SelectByIndex(1);
        
        Click(SubmitRegisterButton);
    }
}
