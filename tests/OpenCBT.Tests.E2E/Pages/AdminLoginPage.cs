using OpenQA.Selenium;

namespace OpenCBT.Tests.E2E.Pages;

public class AdminLoginPage : BasePage
{
    public AdminLoginPage(IWebDriver driver) : base(driver)
    {
    }

    private By EmailInput => By.Id("Input_Email");
    private By PasswordInput => By.Id("Input_Password");
    private By SubmitButton => By.CssSelector("button[type='submit']");
    private By BrandLogo => By.XPath("//*[contains(text(), 'OpenCBT')]");

    public void GoTo(string baseUrl)
    {
        Driver.Navigate().GoToUrl(baseUrl.TrimEnd('/') + "/Account/Login");
        WaitAndFindElement(EmailInput);
    }

    public AdminDashboardPage Login(string baseUrl, string email, string password)
    {
        Type(EmailInput, email);
        Type(PasswordInput, password);
        Click(SubmitButton);
        
        // After login, the system redirects to /. We need to explicitly go to the Admin Dashboard.
        Driver.Navigate().GoToUrl(baseUrl.TrimEnd('/') + "/Admin/Index");
        
        return new AdminDashboardPage(Driver);
    }

    public void LoginAsStudent(string email, string password)
    {
        Type(EmailInput, email);
        Type(PasswordInput, password);
        Click(SubmitButton);
    }
}
