using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Library.Tests.E2E.Selenium;


// Window contexts: every tab/window is represented by a Handle (string identifier for that window/tab)
// The driver talks to one at a time, and switching to a different window moves where your commands
// are landing. Selenium 4 can also open tabs by itself 
public class WindowTests : E2ETestBase
{
    [Fact]
    public void NewWindow_OpensASecondTab_AndSwitchesBack()
    {
        
        Driver.Navigate().GoToUrl("http://localhost:5173/");

        // Before any tab switching occurs we want to make sure the page is actually ready
        new WebDriverWait(Driver, TimeSpan.FromSeconds(4))
            .Until(d => d.FindElements(By.CssSelector("article.card")).Count > 0);

        // Now - lets play with tabs
        // Store the Handle (id) of our original tab
        var originalTab = Driver.CurrentWindowHandle;

        // Next - lets open a new tab
        Driver.SwitchTo().NewWindow(WindowType.Tab);
        Driver.WindowHandles.Should().HaveCount(2);

        // The new tab is blank - we can drive it somewhere and prove
        // the first tab stayed put
        Driver.Navigate().GoToUrl("http://localhost:5173/about");
        Driver.FindElement(By.TagName("h2")).Text.Should().Be("About");

        // Our second tab is now on the About page - the original tab is on the Catlog
        Driver.Close(); //this closes the current tab/window. Driver is now kinda floating there
        Driver.SwitchTo().Window(originalTab); // back to the first tab - on the Catalog
        Driver.FindElement(By.TagName("h2")).Text.Should().Be("Catalog");
        Driver.WindowHandles.Should().HaveCount(1);

    }

    [Fact]
    public void TargetBlankLink_LandsInANewHandle()
    {
        Driver.Navigate().GoToUrl(WidgetUrl);

        var originalTab = Driver.CurrentWindowHandle;

        Driver.FindElement(By.Id("open-about")).Click();

        // A new tab (handle) appears asynchronously - we can poll for that creation
        // completion with an explicit wait. You can NOT implicitly wait for windows/tabs
        // Implicit waits are for elements
        new WebDriverWait(Driver, TimeSpan.FromSeconds(4))
            .Until(d => d.WindowHandles.Count == 2);

        // Grab the new handle 
        var newTab = Driver.WindowHandles.First(h => h != originalTab);
        Driver.SwitchTo().Window(newTab);

        Driver.FindElement(By.TagName("h2")).Text.Should().Be("About");
        Driver.Url.Should().Contain("/about");

        Driver.Close();
        Driver.SwitchTo().Window(originalTab);
    }

    [Fact]
    public void WindowManagment_ReadsAndSetsSize()
    {
        Driver.Navigate().GoToUrl(WidgetUrl);

        // If you ever need to resize your current browser in a test, you can do so. 
        Driver.Manage().Window.Size.Width.Should().Be(1280);

        // Resize our window
        Driver.Manage().Window.Size = new System.Drawing.Size(1024, 768);

        Driver.Manage().Window.Size.Width.Should().Be(1024);
    }

}