using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI; // where our Select class lives
using OpenQA.Selenium.Chrome;

namespace Library.Tests.E2E.Selenium;

public class SelectTests : IDisposable
{
    private readonly ChromeDriver _driver;

    // The csproj copies TestPages/ into the build output - the page
    // sits beside the test dll wherever the test host runs
    private static string WidgetUrl =>
        new Uri(Path.Combine(AppContext.BaseDirectory, "TestPages", 
            "widgets.html")).AbsoluteUri;

    public SelectTests()
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

    // The Select class (Selenium.Support): a wrapper around a normal
    // element object that is specifically used for <select> element driving
    // Eliminates the need for repeated raw click chaining
    [Fact]
    public void SingleSelect_ByTextValueAndIndex()
    {   
        // Finding our element the old fashioned way - passing that to the
        // SelectElement constructor to gain all those helper methods
        var format = new SelectElement(_driver.FindElement(By.Id("format")));
    
        // Three ways to express the same thing. 
        // Match on what the user SEES (text)
        // What the form element POSTS (value)
        // Or, position in the dropdown (index) - brittle
        format.SelectByText("Paperback");
        format.SelectedOption.GetAttribute("value").Should().Be("soft");

        format.SelectByValue("ebook");
        format.SelectedOption.Text.Should().Be("E-book");

        format.SelectByIndex(0);
        format.SelectedOption.Text.Should().Be("Hardcover");

    }

    [Fact]
    public void MultiSelect_AccumulatesAndDeselects()
    {
        var genres = new SelectElement(_driver.FindElement(By.Id("genres")));

        // First - is this properly configured for multi-selection at all?
        genres.IsMultiple.Should().BeTrue();        

        // Select two things
        genres.SelectByText("Databases");
        genres.SelectByText("Web");

        // Assert both got selected
        genres.AllSelectedOptions.Should().HaveCount(2);

        // Besides Displayed and Enabled - on select elements Selected
        // is a state that it's <options> can have
        genres.Options.First(o => o.Text == "Web").Selected.Should().BeTrue();

        genres.DeselectAll(); // Only works for multiselect - throws if used on single select
        genres.AllSelectedOptions.Should().BeEmpty();

    }
}