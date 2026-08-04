using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Library.Tests.E2E.Selenium;

// Screenshots: Mainly an artifact for helping engineers debug
// Can be full page, or targeting a single element. Saved in
// the /bin/ output directory - already .gitignored
public class ScreenshotTests : IDisposable
{
    private readonly ChromeDriver _driver;

    public ScreenshotTests()
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

        _driver.FindElements(By.CssSelector("article.card")).Should().NotBeEmpty();

    }

    public void Dispose()
    {
        _driver.Quit();
    }

    [Fact]
    public void FullPage_SavesAPng()
    {
        // Taking a full page screenshot
        var path = Path.Combine(Directory.GetCurrentDirectory(), "shots", "catalog-page.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);


        // GetScreenshot() captures the viewport; SaveASFile writes a PNG
        _driver.GetScreenshot().SaveAsFile(path);

        File.Exists(path).Should().BeTrue();

    }

    [Fact]
    public void SingleElement_SavesItsOwnPng()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "shots", "first-card.png");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        // Element level capture: just the pixels of ONE element on the page. Typically used
        // to attach an image for a bug/fix ticket for devs
        var card = _driver.FindElement(By.CssSelector("article.card"));

        // This line is gonna do a few things at once
        // First: Casts the IWebElement (our book card) to an ITakesScreenshot
        // needed because not all WebElements expose screenshot methods
        // GetScreenshot() tells the browser to take a screenshot of ONLY the pixels
        // belonging to that DOM element.
        // SaveAsFile saves that data to the file specified in path
        ((ITakesScreenshot) card).GetScreenshot().SaveAsFile(path);

        // Asserting that file now exists
        File.Exists(path).Should().BeTrue();
    }
}