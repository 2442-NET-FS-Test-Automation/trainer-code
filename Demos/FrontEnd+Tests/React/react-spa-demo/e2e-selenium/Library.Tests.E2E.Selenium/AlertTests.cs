using FluentAssertions;
using OpenQA.Selenium;

namespace Library.Tests.E2E.Selenium;

// Browser alerts - alert(), confirm(), prompt() js methods - live outside the DOM.
// No locator can reach them - the driver must switch to the dialog, read it, answer it, etc
// then come back. 

public class AlertTests : E2ETestBase
{
    
    public AlertTests()
    {
        Driver.Navigate().GoToUrl(WidgetUrl);
    }

    [Fact]
    public void PlainAlert_IsReadAndAccepted()
    {
        Driver.FindElement(By.Id("alert-btn")).Click();

        // Switching focus from the DOM to the Alert
        // SwitchTo().Alert() throws a NoAlertPresentException if nothing is up
        var alert = Driver.SwitchTo().Alert();
        alert.Text.Should().Be("Book saved.");

        // If you want to continue working on that page for a workflow 
        alert.Accept();
    }

    [Fact]
    public void Confirm_AcceptAndDismiss()
    {
        // This alert has both an OK and a Cancel button
        // our test can drive both
        Driver.FindElement(By.Id("confirm-btn")).Click();
        Driver.SwitchTo().Alert().Dismiss(); // clicks the cancel button on the alert
        Driver.FindElement(By.Id("confirm-result")).Text.Should().Be("kept");

        // Accept - OK
        Driver.FindElement(By.Id("confirm-btn")).Click();
        Driver.SwitchTo().Alert().Accept(); // clicks the OK button on the Alert
        Driver.FindElement(By.Id("confirm-result")).Text.Should().Be("deleted");
    }

    [Fact]
    public void Prompt_TakesTypedInput()
    {
        Driver.FindElement(By.Id("prompt-btn")).Click();

        var prompt = Driver.SwitchTo().Alert();
        prompt.SendKeys("ada");
        prompt.Accept();

        Driver.FindElement(By.Id("prompt-result")).Text.Should().Be("ada");

    }
}