using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI; // where our Select class lives
using OpenQA.Selenium.Chrome;

namespace Library.Tests.E2E.Selenium;

public class WaitTests : IDisposable
{
    private readonly ChromeDriver _driver;

    public WaitTests()
    {
         // Option classes: per browser launch config.
        // Headless makes it so chrome doesn't pop up
        // we can even tell it things like what window size we want it to use
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1280,900");

        // Creating our driver with the options above
        _driver = new ChromeDriver(options);

        // No more implicit wait

        // Every test in this file will start at the catalog
        _driver.Navigate().GoToUrl("http://localhost:5173/");
    }

    public void Dispose()
    {
        _driver.Quit();
    }

    [Fact]
    public void WithoutAnyWait_ApiRenderRace()
    {
        // Zero waits: Whenever we hit FindElements we look
        // for our target in whatever the DOM holds at that very moment. 
        // The API may not have answered in time - React may still be rendering, etc
        // We may find zero cards - at the moment FindElement fires off. 
        var cards = _driver.FindElements(By.CssSelector("article.card"));

        // If we assert on a specific count (or the presence of cards at all),
        // we may fail
        cards.Should().NotBeEmpty();
    }

    [Fact]
    public void ExplictWait_TargetsOneCondition()
    {
        // WebDriverWait: We give this a condition, and a timeout time
        // It will keep polling for the given time until the condition becomes true
        // If it becomes true - test proceeds. If we hit the timeout - test fails.
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(4));

        var cards = wait.Until(d =>
        {
            var found = d.FindElements(By.CssSelector("article.card"));
            return found.Count > 0 ? found : null;
        });

        cards.Should().NotBeEmpty();
    }

    [Fact]
    public void ExplicitWait_PinsATransientUiState()
    {   
        // Navigate to login page - we will fail a login via wrong password
        _driver.Navigate().GoToUrl("http://localhost:5173/login");

        _driver.FindElement(By.CssSelector("form.login input:not([type='password'])"))
            .SendKeys("ada");

        _driver.FindElement(By.CssSelector("form.login input[type='password']"))
            .SendKeys("wrong-password");

        _driver.FindElement(By.CssSelector("form.login button[type='submit']")).Click();

        // The error paragraph does not exist until the API answers 401 AND React re-renders
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(4));

        var errorParagraph = wait.Until(d =>
        {
            var found = d.FindElements(By.CssSelector("p.error"));
            return found.Count > 0 ? found[0] : null;
        });

        errorParagraph.Text.Should().Be("Invalid username or password.");

    }

    [Fact]
    public void FluentWait_SetPollingIgnoreNoise()
    {
        
        // Fluent waits in .NET are not a separate class. It's a WebDriverWait with 
        // settings tuned. How often to poll, which exceptions to ignore, etc.
        // For example FindElement throws an exception when nothing is found 
        // we can have the wait retry it's code block rather than have the test fail
        var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(4))
        {   
            // Retry the code in our wait every 250 milliseconds
            PollingInterval = TimeSpan.FromMilliseconds(250),
        };

        wait.IgnoreExceptionTypes(typeof(NoSuchElementException));

        var firstCard = wait.Until(d =>
        {
           return d.FindElement(By.CssSelector("article.card h3 a"));
        });

        firstCard.Text.Should().NotBeNullOrEmpty();

    }
}