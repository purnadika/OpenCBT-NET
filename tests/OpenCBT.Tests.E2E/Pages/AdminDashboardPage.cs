using OpenQA.Selenium;

namespace OpenCBT.Tests.E2E.Pages;

public class AdminDashboardPage : BasePage
{
    public AdminDashboardPage(IWebDriver driver) : base(driver)
    {
        WaitAndFindElement(By.XPath("//h1[contains(text(), 'Dashboard')]"));
    }

    public AdminStudentsPage GoToStudents()
    {
        Click(By.XPath("//a[contains(@href, '/Admin/Students')]"));
        return new AdminStudentsPage(Driver);
    }
}
