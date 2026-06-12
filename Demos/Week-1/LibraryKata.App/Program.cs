namespace LibraryKata.App; // A namespace is like a bucket or logical container for different
// related code files.
public class Program
{
    
    // Now we are moving away from the Python file style Top-Level statements
    // So we need a class to hold our Main() method. The previous style with no class
    // or main - implicity had a Main() under the hood. 

    // public - accessible across the program
    // static - Main can be called upon without a Program object. It is a Static/class method. 
    // void - it doesn't return anything
    public static void Main()
    {   
        // When I call dotnet run, it finds Main() and begins code execution at the first line of the 
        // main method. I wrote my code, inside DataTypesAndOperators() - a separate method. So if I want 
        // that code to run, I need to call it inside Main()
        Program.DataTypesAndOperators();
    }

    // private - accessible only within this class
    // static - it belongs to the class, not objects of the class
    // void - returns nothing
    private static void DataTypesAndOperators() // If I had arguments, or inputs for this method,
    { // they would go inside the parenthesis after the method name 
        Console.WriteLine("=== Data types and operators ==");

        // C# is a Strongly typed language
        // We cannot just create variables and shove whatever we want into them like JS or Python
        int copies = 3; // whole numbers
        double lateFee = 1; // floating point numbers (decimals)
        bool isMember = true; // true/false values
        char shelf = 'A'; // single character
        string title = "Clean Code"; // text, strings are reference types

        // Operators 
        string user = "Jon"; // Single = is the assignment operator. 
        int total = copies * 2; // example of an arithmetic operator, like + - * / 
        bool isEnough = total > 4; // comparison - This line compares the value in total to 4, if it is greater
        // than 4, isEnough will get 'true', otherwise it will get 'false'
        // >, <, >=, <= - comparison operators
        bool exactlySix = total == 6; // equality. Single equals is assignment, double equals is equality.
        // unlike JS there is NO === all equality in C# is Strict equality
        bool lendable = isMember && isEnough; //logical operators
        // && - and, || - or, ! - reverses the condition that follows, ^ logical XOR - returns true if ONLY one condition is true

        // This is the basic way to construct strings from other strings
        // String concat - it works! But it can be messy
        Console.WriteLine(title + " has been checked out by " + user);

        // We can create much cleaner formatted strings
        // using String Interpolation - a string with a $ before the opening quote
        Console.WriteLine($"{title} on shelf {shelf}: {copies} copies, fee {lateFee}"); 

        // C# has ALOT of shorthands and little shortcuts that you can find and use 
        // to make your code easier to write. For example, lets say I want to add 1 to the value of total
        // I could do something like
        // total = total + 1; - ORRR
        total += 1; // arithmetic shorthand for the same thing, also works for *= /= -=

    }

    private static void ControlFlow()
    {
        Console.WriteLine("\n== Control Flow ==");


        // If - else if - else
        int copiesAvailable = 0; 
        bool isMember = true;

        if(copiesAvailable > 1 ) 
            Console.WriteLine("Many available for checkout!");
        else if (copiesAvailable == 1) {
            Console.WriteLine("Last copy!");
        }
        else {
            Console.WriteLine("Out of stock!");
            Console.WriteLine("Check again later!");
        }

        // Switch
        string genre = "Mystery";


        // Classic switch - notice C# cares about intent alot! No fall through like in other langauges
        switch (genre)
        {
            case "Mystery":
                Console.WriteLine("Check section A!");
                break;
            case "Science-Fiction":
                Console.WriteLine("Check Section F!");
                break;
            default:  // While optional, a default case to catch any edge cases is best practices
                Console.WriteLine("Uh oh");
                break;
        }

        // New in .NET 8, Switch Expressions! You don't have to use these - they prooobably wont come up in QC
        // but they're used out in real world code, so here is an example. In a switch expression, we want 
        // a return value from the switch - we can then use that value to print out a result
        string section = genre switch
        {
          // This is my expression body
          "Mystery" => "Section A",
          "Science-Fiction" => "Section F",
          _ => "Uh oh" //default  
        };
        Console.WriteLine(section);

    }

    private static void  Loops()
    {
        // C# provides for loops as well, same as Java and any other language
        // For, while, do-while, etc
        for (int day = 1, day <= 3; day++)
        {
            Console.WriteLine($"Reminder day {day}: fee so far {CalculateLateFee(day)}");
        }
        
        int onShelf = 3;
        while (onShelf > 0)
            Console.WriteLine($"{onShelf} copies on the shelf!");
            onShelf--; // quick decrement shorthand

        Console.WriteLine("No copies on shelf!");

        string myString = "dog";

        myString = "cat";
    }

    // I can use this shorthand for one line methods
    private static decimal CalculateLateFee(int daysLate) => daysLate * 2;


    private static void ArraysWork()
    {
        // C# provides for Arrays as well as lists and other collections - we'll get to those later. 
        string[] books = { "Dune", "Harry Potter", "Percy Jackson", "Lord of the Rings" };
        
        Console.WriteLine(books[2]); // I can access indiviudal elements - keeping in mind we index at 0
        
        // C# allows for for-each loops
        foreach (string book in books)
        {
            Console.WriteLine(book);
        }

    }


}