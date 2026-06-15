# OS Fundamentals, the CLI, and File I/O

## Learning Objectives
- Explain what an operating system does for a developer.
- Navigate and manipulate the filesystem from GitBash using core commands.
- Move, copy, and delete files from the command line.
- Read from and write to files in C#.
- Distinguish persistent storage from ephemeral storage.

## Why This Matters
A developer lives in two places: an editor and a terminal. Clicking through folders is fine for casual use, but every tool you will meet later — the .NET CLI, Git, Docker, CI pipelines — is driven from a command line. Being fluent at the shell is the difference between fighting your machine and directing it. And nearly every program eventually has to *remember* something after it closes; file I/O is your first taste of persistence, the idea that underlies databases, logs, and configuration. Today's commands and snippets are small, but they are the muscle memory the rest of the cohort assumes.

## The Concept

### What an OS does for you
The **operating system** (Windows, macOS, Linux) manages the hardware so your programs do not have to: it schedules the CPU, allocates memory, and — most relevant today — organizes the **filesystem** as a tree of directories (folders) and files. The **shell** is a program that reads typed commands and asks the OS to carry them out. On Windows we use **GitBash**, which gives you a Unix-style shell (the same commands used on Linux servers you will deploy to later).

### Navigating the filesystem
Paths describe where you are in the tree. `/` separates folders; `.` is "here", `..` is "one level up".

| Command | Does |
|---------|------|
| `pwd` | print working directory (where am I) |
| `ls` | list files here (`ls -a` shows hidden ones) |
| `cd folder` | change into a folder (`cd ..` goes up) |
| `clear` | clear the screen |

```bash
pwd                 # /c/Users/you/projects
cd FirstName-Lastname
ls -a               # see everything, including .gitignore
```

### Creating, moving, and deleting
| Command | Does |
|---------|------|
| `mkdir name` | make a new directory |
| `touch file.txt` | create an empty file |
| `cp src dest` | copy a file |
| `mv src dest` | move **or rename** a file |
| `rm file` | delete a file (`rm -r folder` for a directory) |

```bash
mkdir notes
touch notes/day1.txt
cp notes/day1.txt notes/backup.txt
mv notes/day1.txt notes/thursday.txt   # rename
rm notes/backup.txt
```

> `rm` is permanent — there is no recycle bin at the shell. Read the line before you press Enter, especially with `rm -r`.

### Reading file contents
| Command | Does |
|---------|------|
| `cat file` | print a whole file |
| `head -n 5 file` | first 5 lines |
| `tail -n 5 file` | last 5 lines |

### Reading and writing files in C#
The .NET `System.IO` namespace makes simple file work a one-liner. The `File` class covers the common cases:

```csharp
using System.IO;

// Write (overwrites the file if it exists)
File.WriteAllText("greeting.txt", "Hello, file!");

// Append instead of overwrite
File.AppendAllText("greeting.txt", "\nA second line.");

// Read it all back as one string
string contents = File.ReadAllText("greeting.txt");
Console.WriteLine(contents);

// Read line by line into an array
string[] lines = File.ReadAllLines("greeting.txt");
Console.WriteLine($"The file has {lines.Length} lines.");
```

`WriteAllText` replaces the file; `AppendAllText` adds to the end; `ReadAllText`/`ReadAllLines` pull it back. (For very large files there are streaming APIs like `StreamReader`, but the `File` helpers are the right default while learning.)

### Persistence vs ephemeral storage
- **Ephemeral** storage lives only while the program runs — your variables, objects, and arrays sit in RAM and vanish when the process exits.
- **Persistent** storage survives the program ending — files on disk, and later, databases.

Writing to a file is your first act of persistence: run the program twice and the file is still there the second time, because it lives on disk, not in memory. This same distinction scales up to why we use databases instead of keeping everything in variables.

## Code Example (When Relevant)
A tiny C# program that persists a run count across executions:

```csharp
using System;
using System.IO;

string path = "runcount.txt";
int count = File.Exists(path) ? int.Parse(File.ReadAllText(path)) : 0;
count++;
File.WriteAllText(path, count.ToString());
Console.WriteLine($"This program has run {count} time(s).");
```

Run it three times and it prints 1, then 2, then 3 — the count persists because it lives in a file, not in memory.

## Summary
- The **OS** manages hardware and the filesystem; the **shell** (GitBash) drives it with typed commands.
- Navigate with `pwd`, `ls`, `cd`; manipulate with `mkdir`, `touch`, `cp`, `mv`, `rm` — `rm` is permanent.
- In C#, `File.WriteAllText` / `AppendAllText` write and `File.ReadAllText` / `ReadAllLines` read.
- **Ephemeral** data (variables in RAM) disappears on exit; **persistent** data (files, databases) survives — file I/O is persistence step one.

## Additional Resources
- [The Unix shell — Software Carpentry](https://swcarpentry.github.io/shell-novice/)
- [File and stream I/O — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/standard/io/)
- [System.IO.File class — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.io.file)
