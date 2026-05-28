using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace OpenCBT.Tests.E2E.Pages;

public abstract class BasePage
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;

    protected BasePage(IWebDriver driver)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    protected IWebElement WaitAndFindElement(By by)
    {
        return Wait.Until(d => {
            try {
                var el = d.FindElement(by);
                return (el != null && el.Displayed && el.Enabled) ? el : null;
            } catch (StaleElementReferenceException) {
                return null;
            } catch (NoSuchElementException) {
                return null;
            }
        });
    }

    protected void Click(By by)
    {
        WaitAndFindElement(by).Click();
    }

    protected void Type(By by, string text)
    {
        var el = WaitAndFindElement(by);
        el.Clear();
        el.SendKeys(text);
    }
}
