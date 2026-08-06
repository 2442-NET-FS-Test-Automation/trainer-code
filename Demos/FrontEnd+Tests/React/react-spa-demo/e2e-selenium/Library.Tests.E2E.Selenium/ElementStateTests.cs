using FluentAssertions;
using OpenQA.Selenium;

namespace Library.Tests.E2E.Selenium;

// Element-state validation, the complete toolkit in one place: Displayed,
// Enabled, Selected, Text, GetAttribute - the five reads every assert about
// "what the user sees" is built from. Wednesday and Thursday used each one in
// passing; this class is the reference card.
public class ElementStateTests : E2ETestBase
{
    public ElementStateTests()
    {
        Driver.Navigate().GoToUrl(WidgetUrl);
    }

    [Fact]
    public void Disabled_IsEnabledFalse_NotInvisible()
    {
        var locked = Driver.FindElement(By.Id("disabled-input"));

        // A user can SEE it and cannot USE it - two different questions,
        // two different reads.
        locked.Displayed.Should().BeTrue();
        locked.Enabled.Should().BeFalse();
        locked.GetAttribute("value").Should().Be("you cannot type here");
    }

    [Fact]
    public void Hidden_IsDisplayedFalse_ButStillInTheDom()
    {
        var menu = Driver.FindElement(By.Id("hover-menu"));

        // FindElement SUCCEEDED - existence and visibility are separate.
        // (Clicking it anyway is ElementNotInteractable - see ExceptionTests.)
        menu.Displayed.Should().BeFalse();
        menu.Enabled.Should().BeTrue();
    }
}