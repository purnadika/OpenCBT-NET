using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace OpenCBT.Tests.E2E.Pages;

public class AdminQuestionsPage : BasePage
{
    public AdminQuestionsPage(IWebDriver driver) : base(driver)
    {
    }

    public void AddMultipleChoiceQuestion(string text, double points, string[] options, int correctOptionIndex)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        
        // Open Modal
        wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Add Question')]"))).Click();
        
        // Wait for modal to appear
        var textArea = wait.Until(d => {
            var el = d.FindElement(By.Id("NewQuestion_Text"));
            return el.Displayed ? el : null;
        });

        textArea.Clear();
        textArea.SendKeys(text);

        var pointsInput = Driver.FindElement(By.Id("NewQuestion_Points"));
        pointsInput.Clear();
        pointsInput.SendKeys(points.ToString());

        // Fill options
        for (int i = 0; i < options.Length; i++)
        {
            var optInput = Driver.FindElement(By.Name($"NewQuestion.Options[{i}].Text"));
            optInput.Clear();
            optInput.SendKeys(options[i]);

            if (i == correctOptionIndex)
            {
                var radios = Driver.FindElements(By.Name("correctIndex"));
                radios[i].Click();
            }
        }

        // Save
        Driver.FindElement(By.XPath("//button[contains(text(), 'Save')]")).Click();
        
        // Wait for the page to reload and the new question to appear in the list
        wait.Until(d => {
            try {
                return d.FindElements(By.XPath($"//p[contains(text(), '{text}')]")).Count > 0;
            } catch (StaleElementReferenceException) {
                return false;
            }
        });
        
        // Let animation finish
        System.Threading.Thread.Sleep(500);
    }

    public void AddEssayQuestion(string text, double points)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        
        // Open Modal
        wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Add Question')]"))).Click();
        
        // Wait for modal to appear
        var textArea = wait.Until(d => {
            var el = d.FindElement(By.Id("NewQuestion_Text"));
            return el.Displayed ? el : null;
        });

        textArea.Clear();
        textArea.SendKeys(text);

        var pointsInput = Driver.FindElement(By.Id("NewQuestion_Points"));
        pointsInput.Clear();
        pointsInput.SendKeys(points.ToString());

        // Remove all options to make it an essay
        var removeButtons = Driver.FindElements(By.XPath("//button[contains(text(), 'Remove')]"));
        // Need to click remove continuously until they are all gone
        while(removeButtons.Count > 0)
        {
            removeButtons[0].Click();
            removeButtons = Driver.FindElements(By.XPath("//button[contains(text(), 'Remove')]"));
        }

        // Save
        Driver.FindElement(By.XPath("//button[contains(text(), 'Save')]")).Click();
        
        // Wait for the page to reload and the new question to appear in the list
        wait.Until(d => {
            try {
                return d.FindElements(By.XPath($"//p[contains(text(), '{text}')]")).Count > 0;
            } catch (StaleElementReferenceException) {
                return false;
            }
        });
        
        // Let animation finish
        System.Threading.Thread.Sleep(500);
    }
}
