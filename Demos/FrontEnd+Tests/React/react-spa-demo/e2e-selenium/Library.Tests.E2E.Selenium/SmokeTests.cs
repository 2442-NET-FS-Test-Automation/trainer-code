using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Library.Tests.E2E.Selenium;

public class SmokeTests : IDisposable
{
    // Our first selenium test. 
    // We need an instance of our driver - matched to our browser
    private readonly ChromeDriver _driver;

    public SmokeTests()
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
    }

    public void Dispose()
    {
        _driver.Quit(); // kills the browser AND the chromedriver process
    }

    [Fact]
    public void OpeningTheSpa_ShowsTitleAndHeading()
    {
        // Act - a real navigation in a real browser
        _driver.Navigate().GoToUrl("http://localhost:5173/");

        // Assert - the document title and the header react renders
        _driver.Title.Should().Be("Library - Catalog");
        _driver.FindElement(By.TagName("h1")).Text.Should().Be("Library");
    }

    [Fact]
    public void Catalog_RendersBookCards_FromTheLiveApi()
    {
        // Act - a real navigation in a real browser
        _driver.Navigate().GoToUrl("http://localhost:5173/");

        // Assert - using the same css selectors as Cypress. 
        var cards = _driver.FindElements(By.CssSelector("article.card"));
        cards.Should().NotBeEmpty();
    }
}