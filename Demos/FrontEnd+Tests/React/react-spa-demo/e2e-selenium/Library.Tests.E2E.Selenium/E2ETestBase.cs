using System.Runtime.CompilerServices;
using OpenQA.Selenium.Chrome;

namespace Library.Tests.E2E.Selenium;

// We have put that driver setup across atleast 8 classes so far
// We can centralize that - we can create a class that some or ALL
// of our tests inherit from, and avoid replicating that code. 

public abstract class E2ETestBase : IDisposable
{
    protected ChromeDriver Driver { get; }

    protected static string WidgetUrl =>
        new Uri(Path.Combine(AppContext.BaseDirectory, "TestPages", 
            "widgets.html")).AbsoluteUri;

    protected E2ETestBase()
    {   
        // Centralized options setup
        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1280,900");

        // Creating our driver with the options above
        Driver = new ChromeDriver(options);
    }

    // Lets also setup screenshot capture-on-failure.
    // We can't put it into the Dispose() because xUnit doesn't tell dispose WHY
    // an object is being dereferenced. So we will make a method that wraps the test body.
    // On any exception, screenshot whatever caused the exception, then rethrow that exception
    // so the test case can fail. 

    // CallerMemberName represents the name of the method that this Guarded method will wrap.
    // It is a compiler attribute that automatically injects the name of the method that 
    // called Guarded() into the testName parameter's value. So it will never actually
    // be an empty string during runtime
    protected void Guarded(Action act, [CallerMemberName] string testName = "")
    {
        try
        {   // Our Action we call act is a delegate - a method. Whatever
            // it might be, in our case an xUnit Selenium test, we try executing it
            act(); 
        }
        catch
        {
            // If we catch an exception during the test method's execution (test failed)
            var path = Path.Combine(
                Directory.GetCurrentDirectory(), "shots", $"FAILED-{testName}.png"
            );

            // Creating the directory if it doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);    

            // We actually take + save the screenshot
            Driver.GetScreenshot().SaveAsFile(path);

            throw; // manually throw the exception - so xUnit can catch it and mark the test failed
        }
    }

    // One limitation of this public void Dispose is that any subclass that might need extra teardown
    // has no clean way to extend this. We could use a virtual method instead if we wanted to. 
    public void Dispose()
    {
        Driver.Quit();
    }

}
