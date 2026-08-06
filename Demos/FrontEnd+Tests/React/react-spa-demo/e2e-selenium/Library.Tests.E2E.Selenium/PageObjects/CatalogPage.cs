using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Library.Tests.E2E.Selenium.PageObjects;

// Page Object Model - same idea as Cypress'
// One class per page, selectors and any page actions
// in one place. This class will be a mirror 
// of cypress/pages/CatalogPage.js
public class CatalogPage
{
    private readonly IWebDriver _driver;

    // Define selectors in this page - and avoid retyping again and again
    private static readonly By Cards = By.CssSelector("article.card");
    private static readonly By SearchBox = By.CssSelector("input[type='search']");
    private static readonly By SortButton = By.CssSelector(".toolbar button");
    private static readonly By FirstTitleLink = By.CssSelector("article.card h3 a");
    private static readonly By SignedInLabel = By.CssSelector(".auth-box span");

    public CatalogPage(IWebDriver driver)
    {
        _driver = driver;
    }

    // Page actions - methods
    // These methods return a catalog page... kind of. 
    public CatalogPage Visit()
    {
        _driver.Navigate().GoToUrl("http://localhost:5173");

        // We add it's readiness condition (the wait) to the POM method
        // the caller should not have to wait for this method 
        new WebDriverWait(_driver, TimeSpan.FromSeconds(4))
            .Until(d => d.FindElements(Cards).Count > 0);

        // return this allows for method chaining
        return this;
    }

    public CatalogPage Search(string text)
    {
        _driver.FindElement(SearchBox).SendKeys(text);
        return this;
    }

    public CatalogPage ToggleSort()
    {
        _driver.FindElement(SortButton).Click();
        return this;
    }

    // We can also set up properties about the page's state 
    // that we can then read from as if they were like Element.Text
    // State Retrieval Properties
    public int CardCount => _driver.FindElements(Cards).Count;
    public string FirstTitle => _driver.FindElement(FirstTitleLink).Text;
    public string SignedInUser => _driver.FindElement(SignedInLabel).Text;
}