using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.BiDi.BrowsingContext;
using OpenQA.Selenium.Chrome;

namespace Library.Tests.E2E.Selenium;


// Locator demo - this is how your tests can navigate your SPA. 
// FindElement finds the FIRST match or throws an exception
// FindElements returns all matches or an EMPTY list.
public class LocatorTests : IDisposable
{
    
    private readonly ChromeDriver _driver;

    public LocatorTests()
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

    // By tag name returns the first h1 wherever it is on our page. Fine
    // for now because we only have 1 - potentially wrong if we add another. 
    [Fact]
    public void ByTagName_FindsTheHeader()
    {
        _driver.FindElement(By.TagName("h1")).Text.Should().Be("Library");
    }

    [Fact]
    public void ByClassName_FindsEveryCard()
    {
        // One class token - "card" not "article.card"
        var cards = _driver.FindElements(By.ClassName("card"));

        cards.Should().NotBeEmpty();
    }

    [Fact]
    public void ByCssSelector_ComposesStructureAndClass()
    {
        // The go-to: same selector language that CSS (and Cypress) used
        var firstTitleLink = _driver.FindElement(By.CssSelector("article.card h3 a"));

        firstTitleLink.Text.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ByLinkText_FindsAnchorsByWhatUserReads()
    {
        // LinkText matches <a> elements ONLY, by their exact visible text
        // PartialLinkText does this by a substring. 
        _driver.FindElement(By.LinkText("About")).TagName.Should().Be("a");
        _driver.FindElement(By.PartialLinkText("Cata")).Text.Should().Be("Catalog");

    }


}