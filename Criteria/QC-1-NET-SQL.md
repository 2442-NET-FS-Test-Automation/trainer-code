# QC 1 (.NET) Criteria

## C#/.NET

| Priority | Objective | Example / Explanation |
| :--- | :--- | :--- |
| Must know | Utilize the dotnet command line tools to generate and execute projects. | `dotnet new console -n MyApp`<br>`dotnet run` |
| Must know | Create methods that allow for the reusability of code. | `public int Add(int a, int b) { return a + b; }` |
| Must know | Describe the purpose and structure of a unit test. | Ensures individual units of code work as expected; typically structured using the Arrange, Act, Assert (AAA) pattern. |
| Must know | Create a functional application to fulfill behavioural requirements and user stories. | Developing a complete program that meets specific acceptance criteria (e.g., processing user input to yield a specified result). |
| Must know | Can describe the .NET compilation process and its steps. | Source code (.cs) compiles into Intermediate Language (IL), which the CLR's Just-In-Time (JIT) compiler then converts to native machine code at runtime. |
| Must know | Demonstrate proper syntax for working with arrays. | `int[] numbers = new int[3] { 1, 2, 3 };` |
| Must know | Effectively interprets stack traces in order to debug code files. | Reading exception output from top to bottom to identify the specific file, method, and line number where a crash originated. |
| Must know | Utilizes control flow where appropriate to achieve desired behavior during runtime. | `if (condition) { /* do A */ } else { /* do B */ }` |
| Must know | Uses Try-Catch-Finally to avoid hard crashing when running "risky" operations. | `try { /* network call */ } catch (Exception ex) { Console.WriteLine(ex); } finally { /* cleanup */ }` |
| Must know | Understands and can explain the four pillars of OOP. | Encapsulation (data hiding), Inheritance (code reuse), Polymorphism (method overriding/overloading), and Abstraction (hiding implementation complexity). |
| Must know | Can describe the role of the .NET SDK and its use in development. | A bundle of tools, libraries, and compilers (including the `dotnet` CLI) required to build and test .NET applications. |
| Must know | Can initialize and run a console application using the .NET CLI. | `dotnet new console`<br>`dotnet run` |
| Must know | Can identify and use basic data types appropriately. | `int age = 30; string name = "John"; bool isValid = true;` |
| Must know | Can use basic, comparison, equality, and logical operators in programming logic. | `if (age >= 18 && hasID == true)` |
| Must know | Can differentiate between value and reference types, and describe stack vs. heap allocation. | Value types (e.g., int, struct) hold data directly on the stack. Reference types (e.g., string, class) hold a memory address on the stack that points to the data on the heap. |
| Must know | Can model real-world entities using classes, fields, methods, and constructors. | `public class Car { public string Make; public Car(string m) { Make = m; } }` |
| Must know | Describe the purpose of a design pattern. | A standardized, reusable solution to a commonly occurring problem within a given context in software design. |
| Must know | Describe the purpose and differences of collections. | Data structures used to store objects. Arrays have fixed sizes, Lists are dynamic, and Dictionaries store key-value pairs for fast lookups. |
| Must know | Implement type conversion in an application. | `int number = Convert.ToInt32("123");` |
| Must know | Describe and utilize SOLID principles in application design. | Five architectural principles (Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion) to ensure maintainability. |
| Must know | Utilize the HttpClient object to make HTTP calls to external APIs. | `using var client = new HttpClient(); var data = await client.GetStringAsync("https://api.example.com");` |
| Must know | Demonstrates understanding of data structures and collections in C#. | Knowing when performance dictates the use of a `Queue<T>` (FIFO), `Stack<T>` (LIFO), or `HashSet<T>` (unique elements). |
| Must know | Demonstrate understand of Generic Types in C#. | `public class Box<T> { public T Content { get; set; } }` |
| Must know | Program asynchronously in C# using async and await. | `public async Task<string> FetchAsync() { await Task.Delay(1000); return "Done"; }` |
| Must know | Describe the functionality of different sorting algorithms. | Understanding the mechanics and performance of approaches like Bubble Sort (swapping adjacent), Merge Sort (divide and conquer), and Quick Sort (pivot partitioning). |
| Must know | Understand and discuss Service Oriented Architecture and Microservices. | Architectural styles that structure an application as a collection of loosely coupled, independently deployable services. |
| Must know | Describe asymptotic and Big-O notation, and how application logic can be written efficiently. | Mathematical notation defining algorithmic complexity (e.g., O(1) for constant time, O(N) for linear time, O(N^2) for quadratic). |
| Should know | Use encapsulation and abstration in applications, with appropriate access modifiers and modifiers on classes and methods. | `private int _age; public void SetAge(int age) { if (age > 0) _age = age; }` |
| Should know | Use inheritance and polymorphism to create classes that have inherited members, and overrides or overloads members as necessary. | `public class Dog : Animal { public override void Speak() { Console.WriteLine("Bark"); } }` |
| Should know | Leverages manually thrown exceptions and bubbling to debug business logic. | `if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name cannot be null");` |
| Should know | Understands auto-property syntax for class fields. | `public string Name { get; set; }` |
| Should know | Can describe the difference between an Interface and Abstract class, and can appropriately leverage either as needed in their program. | Interfaces define a contract without implementation (multiple interfaces allowed per class). Abstract classes can contain implemented logic but cannot be directly instantiated. |
| Should know | Understands the Primary Constructor syntax for classes. | `public class User(string Name, int Age);` |
| Should know | Can explain how garbage collection works in .NET and avoid common memory leaks. | The CLR periodically reclaims memory from objects no longer in use. Leaks are avoided by unsubscribing from events and disposing of unmanaged resources. |
| Should know | Uses the NuGet Package Manager to install and manage dependencies. | `dotnet add package Newtonsoft.Json` |
| Should know | Can organize applications using solutions and multi-project setups. | Creating a `.sln` file to manage multiple related `.csproj` files (e.g., separating Web API, Business Logic, and Tests). |
| Should know | Can differentiate between static and instance members and explain when to use each. | Static members belong to the class itself and are shared globally; instance members belong to a specific instantiated object. |
| Should know | Describe the repository design pattern. | An abstraction layer between the data access logic and the business logic, providing a centralized way to query the database. |
| Should know | Describe the singleton design pattern. | Restricts the instantiation of a class to one single instance globally and provides a static access point to it. |
| Should know | Describe the unit-of-work design pattern. | Manages a set of operations that alter the database and ensures they are committed as a single transaction. |
| Should know | Implement nullable types in an application. | `int? age = null; if (age.HasValue) { /* process */ }` |
| Should know | Utilize the collections namespace, and types that extend the IEnumerable interface in an application. | `IEnumerable<int> numbers = new List<int> { 1, 2, 3 };` |
| Should know | Implement the Repository pattern in an application. | `public class UserRepository : IUserRepository { public User Get(int id) { /* Db call */ } }` |
| Should know | Implement sorting algorithms in an application. | Constructing a custom `for` loop iteration logic to sort an array, or utilizing built-in methods like `Array.Sort()`. |
| Should know | Implement lambda expressions in an application. | `Func<int, int> square = x => x * x;` |
| Nice to Have | Can create and use their own reusable utility or helper class library. | Abstracting shared string manipulation logic into a `StringUtils` class located in a separate `.dll` for use across multiple applications. |
| Nice to Have | Able to simulate multiple inheritance. | Implementing multiple interfaces on a single class (e.g., `public class Drone : IFlyable, ICamera`). |
| Nice to Have | Can create custom exceptions in their applications to fit specific use cases. | `public class UserNotFoundException : Exception { public UserNotFoundException(string msg) : base(msg) {} }` |
| Nice to Have | Can construct and use class libraries and reference them in multi-project solutions. | `dotnet add reference ../MyLibraryProject/MyLibrary.csproj` |
| Nice to Have | Can use partial classes to split functionality across multiple files. | `public partial class Employee { }` // File 1<br>`public partial class Employee { }` // File 2 |
| Nice to Have | Can apply sealed classes to enforce class design decisions. | `public sealed class SecurityConfig { }` // Prevents other classes from inheriting from this class. |
| Nice to Have | Implement implicit typing using "var." | `var user = new User();` |
| Nice to Have | Implement a lambda expression to perform filters and sorts. | `var activeUsers = users.Where(u => u.IsActive).OrderBy(u => u.Name);` |
| Nice to Have | Demonstrate the implementation of a design pattern. | Implementing a thread-safe Singleton using `Lazy<T>` to guarantee initialization occurs only once. |
| Nice to Have | Demonstrate an understanding of REGEX and pattern matching syntax. | `bool isValid = Regex.IsMatch(input, @"^\d{3}-\d{2}-\d{4}$");` |
| Nice to Have | Implement recursion in an application. | `public int Factorial(int n) => n == 0 ? 1 : n * Factorial(n - 1);` |

