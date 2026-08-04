using FluentAssertions;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Library.Tests.E2E.Selenium;

// https://devhints.io/xpath


// XPath - stands for XML Path Language - thankfully we don't have to use XML
// in order to use it. Lets us query elements as if they were filepaths on the DOM
// Can match on text, walk UP the DOM tree (CSS can only walk down), pick siblings by 
// position, etc. Two kinds of paths, RELATIVE and ABSOLUTE. Absolute paths
// are rarely, if ever used. 
// Absolute paths start with (/html/the/rest/of/my/path)
// Relative paths start with (//) - double slashes
public class XPathTests : IDisposable
{
     private readonly ChromeDriver _driver;

    public XPathTests()
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
    public void RelativeXPath_MatchesByAttribute()
    {
        // "//" means "anywhere in the document"


        // <article class="card"></article>
        // This would match an article with more than one class applied
        // <article class="card book some-other-class"></article>
        var cards = _driver.FindElements(By.XPath("//article[@class='card']"));

        cards.Should().NotBeEmpty();
    }

    [Fact]
    public void XPathFunctions_MatchOnText()
    {
        // Find the <a> inside the <h3> who's text content contains the string "Clean"
        // Not possible without xpath
        var cleanCode = _driver.FindElement(By.XPath("//h3/a[contains(text(), 'Clean')]"));
        cleanCode.Text.Should().Be("Clean Code");

    
        var skus = _driver.FindElements(By.XPath("//dd[starts-with(text(), 'BK-')]"));
        skus.Should().HaveCount(3);
    }

    [Fact]
    public void XPathAxes_WalkUpAndSideways()
    {
        // Axes define the structural relationship between the current node we start at 
        // called the "context node" and the target nodes in the DOM
        // Relative locations with an axis look like this
        //context-node/axis-name::target-node[predicate]
        // There are 13 axis that you can use in Xpath
        // self - the current context node itself
        // parent - the single immediate parent of the current node
        // child - the single immediate children of the current node
        // ancestor - ALL parents, parents of parents, etc - all the way to the HTML root
        // ancestor-or-self - the above PLUS the current node
        // descendant - all children, their children, etc.
        // descendant-or-self - same as above plus current node 
        // following - the node that appears AFTER the current on - no descendants
            // <div>
            //      <p></p>
            // </div>
            // <p>
            // <p>


        // Climbing from some known text on the page to grab the container element it lives in
        // body > div > article > h3 > a
        var cardOfCleanCode = _driver.FindElement(
            By.XPath("//a[text()='Clean Code']/ancestor::article")
        );

        cardOfCleanCode.GetAttribute("class").Should().Be("card");

        // following-sibling:: 
        var firstSku = _driver.FindElement(
            // Find that first Sku <dt> - then find it's first sibling <dd>
            By.XPath("//dt[text()='SKU']/following-sibling::dd[1]")
        );

        firstSku.Text.Should().Be("BK-001");
    }

    [Fact] // don't ever do this
    public void AbsoluteXPath_WorksToday()
    {
        // Every step in this xpath is a hard dependency on the DOM's exact shape
        // wrap something in one more div, and the selector breaks.
        var h1 = _driver.FindElement(
            By.XPath("/html/body/div/div/header/h1")
        );

        h1.Text.Should().Be("Library");
    }
}