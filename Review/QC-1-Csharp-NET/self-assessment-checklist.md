# QC-1 (.NET) — Self-Assessment Checklist

Every objective below is reproduced **verbatim** from `qc-criteria/QC-1-NET.md`, grouped by priority tier
and topic. Read each as a self-question — *"Can I confidently do / explain this?"* Tick only what you can
do without notes. Each unticked box points you at the matching cluster in `study-guide.md` and task in
`drills.md`.

The wording is the rubric's own (including its phrasing and typos) so this list mirrors the actual exam
criteria exactly. Items the curriculum schedules for later weeks are pulled out into the final section so
the full rubric stays visible while scope is honest.

---

## Must know

### .NET platform & tooling
- [ ] Utilize the dotnet command line tools to generate and execute projects.
- [ ] Can describe the role of the .NET SDK and its use in development.
- [ ] Can initialize and run a console application using the .NET CLI.
- [ ] Can describe the .NET compilation process and its steps.

### C# fundamentals
- [ ] Create methods that allow for the reusability of code.
- [ ] Demonstrate proper syntax for working with arrays.
- [ ] Utilizes control flow where appropriate to achieve desired behavior during runtime.
- [ ] Can identify and use basic data types appropriately.
- [ ] Can use basic, comparison, equality, and logical operators in programming logic.
- [ ] Implement type conversion in an application.

### Memory model
- [ ] Can differentiate between value and reference types, and describe stack vs. heap allocation.

### Object-oriented programming
- [ ] Understands and can explain the four pillars of OOP.
- [ ] Can model real-world entities using classes, fields, methods, and constructors.

### Collections & generics
- [ ] Describe the purpose and differences of collections.
- [ ] Demonstrates understanding of data structures and collections in C#.
- [ ] Demonstrate understand of Generic Types in C#.

### Exceptions & debugging
- [ ] Uses Try-Catch-Finally to avoid hard crashing when running "risky" operations.
- [ ] Effectively interprets stack traces in order to debug code files.

### Design patterns & SOLID
- [ ] Describe the purpose of a design pattern.
- [ ] Describe and utilize SOLID principles in application design.

### Async & networking
- [ ] Utilize the HttpClient object to make HTTP calls to external APIs.
- [ ] Program asynchronously in C# using async and await.

### Building an application
- [ ] Create a functional application to fulfill behavioural requirements and user stories.

---

## Should know

### Object-oriented programming
- [ ] Use encapsulation and abstration in applications, with appropriate access modifiers and modifiers on classes and methods.
- [ ] Use inheritance and polymorphism to create classes that have inherited members, and overrides or overloads members as necessary.
- [ ] Understands auto-property syntax for class fields.
- [ ] Can describe the difference between an Interface and Abstract class, and can appropriately leverage either as needed in their program.
- [ ] Understands the Primary Constructor syntax for classes.
- [ ] Can differentiate between static and instance members and explain when to use each.

### Memory model
- [ ] Can explain how garbage collection works in .NET and avoid common memory leaks.

### Exceptions
- [ ] Leverages manually thrown exceptions and bubbling to debug business logic.

### .NET platform & tooling
- [ ] Uses the NuGet Package Manager to install and manage dependencies.
- [ ] Can organize applications using solutions and multi-project setups.

### Design patterns
- [ ] Describe the repository design pattern.
- [ ] Describe the singleton design pattern.
- [ ] Describe the unit-of-work design pattern.
- [ ] Implement the Repository pattern in an application.

### Collections & language
- [ ] Implement nullable types in an application.
- [ ] Utilize the collections namespace, and types that extend the IEnumerable interface in an application.
- [ ] Implement lambda expressions in an application.

---

## Nice to Have

### .NET platform & class libraries
- [ ] Can create and use their own reusable utility or helper class library.
- [ ] Can construct and use class libraries and reference them in multi-project solutions.

### Object-oriented programming
- [ ] Able to simulate multiple inheritance.
- [ ] Can use partial classes to split functionality across multiple files.
- [ ] Can apply sealed classes to enforce class design decisions.

### C# fundamentals
- [ ] Implement implicit typing using "var."
- [ ] Implement recursion in an application.

### Exceptions
- [ ] Can create custom exceptions in their applications to fit specific use cases.

### Collections & language
- [ ] Implement a lambda expression to perform filters and sorts.

### Design patterns
- [ ] Demonstrate the implementation of a design pattern.

### Regex & pattern matching
- [ ] Demonstrate an understanding of REGEX and pattern matching syntax.

---

## Not yet covered (taught in a later week)

These rubric objectives are **not taught in Weeks 1–2**. They are reproduced here so the full rubric stays
visible, but there is no study material for them in this package — see `out-of-scope-register.md`. Do not
count a missing tick here against your Week 1–2 readiness.

### Must know
- [ ] Describe the purpose and structure of a unit test. *(taught Week 8 — xUnit / Arrange-Act-Assert)*
- [ ] Describe the functionality of different sorting algorithms. *(taught Week 4)*
- [ ] Describe asymptotic and Big-O notation, and how application logic can be written efficiently. *(taught Week 4)*
- [ ] Understand and discuss Service Oriented Architecture and Microservices. *(taught Week 5 / Week 7)*

### Should know
- [ ] Implement sorting algorithms in an application. *(taught Week 4)*
