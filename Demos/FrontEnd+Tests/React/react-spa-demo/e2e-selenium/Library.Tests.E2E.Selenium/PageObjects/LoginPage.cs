using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Library.Tests.E2E.Selenium.PageObjects;

// The login page object. It's sign in action returns the CatalogPage object
// because that is where a successful sign in redirects the user to. 
// Page transitions can be baked into your POM classes
public class LoginPage
{
    // Store any driver we happen to be using
    private readonly IWebDriver _driver;

    // Selectors
    private static readonly By Username = By.CssSelector("form.login input:not([type='password'])");
    private static readonly By Password = By.CssSelector("form.login input[type='password']");
    private static readonly By Submit = By.CssSelector("form.login button[type=submit]");

    public LoginPage(IWebDriver driver)
    {
        _driver = driver;
    }

    // Actions
    public LoginPage Visit()
    {
        _driver.Navigate().GoToUrl("http://localhost:5173/login");
        return this;
    }

    // Sign in method
    public CatalogPage SignInAs(string username, string password)
    {
        _driver.FindElement(Username).SendKeys(username);
        _driver.FindElement(Password).SendKeys(password);
        _driver.FindElement(Submit).Click();

        // Once we do this - we do hit the API. We should explicitly wait for that
        new WebDriverWait(_driver, TimeSpan.FromSeconds(5))
            .Until(d => d.FindElements(By.CssSelector(".auth-box span")).Count > 0);

        return new CatalogPage(_driver);
    }
}