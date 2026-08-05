using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Library.Tests.E2E.Selenium;

public class InteractionTests : IDisposable
{
    private readonly ChromeDriver _driver;

    public InteractionTests()
    {
         // Option classes: per browser launch config.
        // Headless makes it so chrome doesn't pop up
        // we can even tell it things like what window size we want it to use
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1280,900");

        // Creating our driver with the options above
        _driver = new ChromeDriver(options);

        // We can also use the constructor to configure an implicit wait
        // We will set it so each FindElement(s) retries for up to 2s before 
        // failing. Proper explicit waits will be demoed later on.
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);

        // Every test in this file will start at the catalog
        _driver.Navigate().GoToUrl("http://localhost:5173/");
    }

    public void Dispose()
    {
        _driver.Quit();
    }

    [Fact]
    public void LoginForm_SignsIn_ThroughTheUi()
    {   
        // Go to the login page (we start every test from the catalog as per
        // the constructor)
        _driver.Navigate().GoToUrl("http://localhost:5173/login");

        // Our SPA has no id or name attributes - so we're gonna use ALOT
        // of CSS selectors. Hint: you should add those ID selectors
        var username = _driver.FindElement( // could also just ask for the input we named username
            By.CssSelector("form.login input:not([type='password'])"));
        var password = _driver.FindElement(
            By.CssSelector("form.login input[type='password']"));
        var submit = _driver.FindElement(By.CssSelector("form.login button[type='submit']"));

        // Drive the elements.
        username.SendKeys("ada");
        password.SendKeys("pass123!");
        submit.Click();

        // We've entered a valid username + password, and clicked the login button
        // We should be logged in -> redirected to catalog -> greeting text renders
        var who = _driver.FindElement(By.CssSelector(".auth-box span"));
        who.Text.Should().Be("ada (admin)");
    }

    [Fact]
    public void SendKeysAndClear_DriveAControlledInput()
    {
        var search = _driver.FindElement(By.CssSelector("input[type='search']"));

        // The same way we can read text content - we can read attiributes or properties
        search.GetAttribute("placeholder").Should().Be("Filter by name...");

        search.SendKeys("clean");
        search.GetAttribute("value").Should().Be("clean");

        // Lets see if the filter actually worked
        _driver.FindElements(By.CssSelector("article.card")).Should().HaveCount(1);

        // We can then clear the input field - search again, assert more, etc
        search.Clear();
        search.GetAttribute("value").Should().Be("");
    }

    [Fact]
    public void DisplayedAndEnabled_ReadElementState()
    {
        var heading = _driver.FindElement(By.TagName("h2"));

        // Displayed = a user can see it
        // Enabled = a user can operate/interact with it 
        // Both let us validate element state
        heading.Displayed.Should().BeTrue();
        heading.Text.Should().Be("Catalog");

        _driver.FindElement(By.CssSelector(".toolbar button")).Enabled.Should().BeTrue();
    }

}