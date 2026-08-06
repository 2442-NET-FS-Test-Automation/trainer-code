using FluentAssertions;
using OpenQA.Selenium;

namespace Library.Tests.E2E.Selenium;

// The Selenium exception bestiary, triggered ON PURPOSE. Debugging a Selenium
// failure is 90% recognizing which of these you are looking at and what it
// accuses: the locator, the timing, the page lifecycle, or the environment.
public class ExceptionTests : E2ETestBase
{
    [Fact]
    public void NoSuchElement_AccusesTheLocator()
    {
        Driver.Navigate().GoToUrl("http://localhost:5173/");

        // "Your locator matched nothing." No implicit wait in the base, so the
        // throw is immediate - misses fail fast under the explicit strategy.
        var lookup = () => Driver.FindElement(By.CssSelector("#totally-not-there"));

        lookup.Should().Throw<NoSuchElementException>();
    }

    [Fact]
    public void StaleElementReference_AccusesThePageLifecycle()
    {
        Driver.Navigate().GoToUrl("http://localhost:5173/");
        // The header h1 renders synchronously - findable with no wait at all.
        var heading = Driver.FindElement(By.TagName("h1"));

        // The element WAS found - then the document it lived in went away.
        // Any re-render or reload orphans old references; re-find, never hoard.
        Driver.Navigate().Refresh();

        var read = () => heading.Text;
        read.Should().Throw<StaleElementReferenceException>();
    }

    [Fact]
    public void ElementNotInteractable_AccusesVisibility()
    {
        Driver.Navigate().GoToUrl(WidgetUrl);

        // The hover menu EXISTS in the DOM but is display:none - findable,
        // not clickable. Exists and interactable are different questions.
        var hiddenMenu = Driver.FindElement(By.Id("hover-menu"));

        var click = () => hiddenMenu.Click();
        click.Should().Throw<ElementNotInteractableException>();
    }

    [Fact]
    public void WebDriverTimeout_AccusesTheCondition()
    {
        Driver.Navigate().GoToUrl("http://localhost:5173/");

        // An explicit wait whose condition never comes true reports the
        // timeout, not the element - read the .Message for what it polled.
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(2));

        var until = () => wait.Until(d => {
            var found = d.FindElements(By.Id("never-appears"));
            return found.Count > 0 ? found : null;
        });

        until.Should().Throw<WebDriverTimeoutException>();
    }
}