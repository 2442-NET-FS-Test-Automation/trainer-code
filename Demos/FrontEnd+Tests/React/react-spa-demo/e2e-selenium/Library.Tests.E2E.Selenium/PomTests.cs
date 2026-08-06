using FluentAssertions;
using Library.Tests.E2E.Selenium.PageObjects;

namespace Library.Tests.E2E.Selenium;

// Fully POM-d out tests. No waits. No URLs. No selectors.
// Should do everything Interactiontests.cs does but less painfully

public class PomTests : E2ETestBase
{
    
    [Fact]
    public void Filter_ThroughThePageObject()
    {
        var catalog = new CatalogPage(Driver).Visit().Search("clean");

        catalog.CardCount.Should().Be(1);
        catalog.FirstTitle.Should().Be("Clean Code");
    }

    [Fact]
    public void Sort_ThroughThePageObject()
    {
        var catalog = new CatalogPage(Driver).Visit().ToggleSort();

        catalog.FirstTitle.Should().Be("The Pragmatic Programmer");
    }

    [Fact]
    public void SignIn_AcrossPages()
    {
        // Lets use that guard this time. In E2ETestBase we created that Guarded()
        // method for screenshots on failure.
        Guarded(() =>
        {
            // LoginPage SignIn method hands us back a catalog page
            var catalog = new LoginPage(Driver).Visit().SignInAs("ada", "pass123!");

            catalog.SignedInUser.Should().Be("ada (admin)");
        });
    }

}