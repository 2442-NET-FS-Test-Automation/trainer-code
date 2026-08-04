using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Library.Tests.E2E.Selenium;

// Navigation methods - we've seen GoToUrl() for navigating to a specific page
// We have others that lets us navigate like a user would in browser
// GoToUrl(), Back(), Forward(), Refresh() refreshes the page - can be important for SPAs
public class NavigationTests: IDisposable
{
    private readonly ChromeDriver _driver;

    public NavigationTests()
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
    public void DirectUrl_LoadsADeepRoute()
    {
        // Lets go to a BookDetail page for one of our books
        _driver.Navigate().GoToUrl("http://localhost:5173/inventory/BK-001");

        _driver.FindElement(By.TagName("h2")).Text.Should().Be("Clean Code");
    }

    [Fact]
    public void BackForwardRefresh_WalkTheHistory()
    {
        _driver.Navigate().GoToUrl("http://localhost:5173/"); // go to catalog
        _driver.Navigate().GoToUrl("http://localhost:5173/about"); // go to about page

        _driver.FindElement(By.TagName("h2")).Text.Should().Be("About");

        // Back and Forward navigation use the browser's real history stack
        // same as user pressing back and forward buttons on their browser
        _driver.Navigate().Back(); // back to catalog
        _driver.FindElement(By.TagName("h2")).Text.Should().Be("Catalog");

        _driver.Navigate().Forward(); // forward to about
        _driver.FindElement(By.TagName("h2")).Text.Should().Be("About");


        // Refresh reloads the document; the url remains the same because
        // that's our SPA behavior
        _driver.Navigate().Refresh(); // refresh the about page
        _driver.FindElement(By.TagName("h2")).Text.Should().Be("About");
        _driver.Url.Should().EndWith("/about");

    }
}