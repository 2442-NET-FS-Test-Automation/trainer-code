using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V148.IndexedDB;
using OpenQA.Selenium.Interactions; // Actions API

namespace Library.Tests.E2E.Selenium;

// The Actions API (OpenQA.Selenium.Interactions): a builder for compound gestures
// things like - hover, double click, click and hold, keyboard chords (holding a key down), etc
// We can build the action, one step at a time, and then fire it via Perform(). 
// Something like Element.Click() is a single action, Actions are a compound executed in sequence
public class ActionsTests : IDisposable
{
    private readonly ChromeDriver _driver;

    // The csproj copies TestPages/ into the build output - the page
    // sits beside the test dll wherever the test host runs
    private static string WidgetUrl =>
        new Uri(Path.Combine(AppContext.BaseDirectory, "TestPages", 
            "widgets.html")).AbsoluteUri;

    public ActionsTests()
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


        _driver.Navigate().GoToUrl(WidgetUrl);
    }

    public void Dispose()
    {
        _driver.Quit();
    }

    [Fact]
    public void Hover_RevealsTheMenu()
    {   
        // This selects the menu itself - which is not displayed until a user hovers over 
        // another element
        var menu = _driver.FindElement(By.Id("hover-menu"));
        menu.Displayed.Should().BeFalse(); // hidden until :hover

        // We don't want to click on the element we want to hover over. 
        // We can use MoveToElement - moves mouse position over the targeted element
        new Actions(_driver)
            .MoveToElement(_driver.FindElement(By.Id("hover-zone")))
            .Perform();

        menu.Displayed.Should().BeTrue();
        menu.Text.Should().Be("Now you see the menu");
    }

    [Fact]
    public void DoubleClick_FiresTheDblClickEvent()
    {
        new Actions(_driver)
            .DoubleClick(_driver.FindElement(By.Id("dbl-btn")))
            .Perform();

        _driver.FindElement(By.Id("dbl-count")).Text.Should().Be("1");
    }

    [Fact]
    public void KeyboardChord_TypesUppercaseWithShift()
    {
        var input = _driver.FindElement(By.Id("keys-input"));

        // Lets create a keyboard chord. 
        // KeyDown lets you hold a modifier key, you can then continue to chain actions
        // until you call KeyUp. 
        new Actions(_driver)
            .Click(input)
            .KeyDown(Keys.Shift)
            .SendKeys("ada")
            .KeyUp(Keys.Shift)
            .Perform();

        input.GetAttribute("value").Should().Be("ADA");
    }
}