# Git Fundamentals

## Learning Objectives
- Explain what version control is and why it exists.
- Initialize a local Git repository and understand the three states of a file.
- Stage and commit changes with meaningful messages.
- Connect a local repository to a remote and push to it.
- Use a `.gitignore` to keep junk out of the repository.

## Why This Matters
Every deliverable in this cohort is submitted as a Git commit or pull request, and every professional team builds on top of Git. It is the universal "save game with history" for code: it lets you undo mistakes, see who changed what and why, and combine work from many people without overwriting each other. Today you will create your personal `FirstName-Lastname` repository and push to it — the single repo that holds all your work this cohort, one project folder per deliverable. Get comfortable with the core loop — *change, stage, commit, push* — now, because you will run it hundreds of times before this cohort ends. (Branching, pull requests, and merge strategies come later in Week 3; today is the foundation.)

## The Concept

### What version control solves
Without version control, "saving" means overwriting yesterday's file, and collaboration means emailing `final_v2_REALLY_final.zip` around. **Git** is a *distributed version control system*: every clone of a repository carries the full history, so you can work offline, review past states, and reconcile changes safely.

### The repository and the three states
A **repository** ("repo") is a project folder that Git tracks. Git manages a file through three areas:

- **Working directory** — your actual files on disk, where you edit.
- **Staging area (index)** — a holding zone for the changes you want in the *next* commit.
- **Repository (.git)** — the committed history of snapshots.

```
edit files            git add             git commit
working dir  ------>  staging area  ------>  repository (history)
```

A **commit** is a snapshot of the staged changes plus a message describing *why*. Commits form a chain — the project's history.

### Initializing a repository
Inside your project folder:

```bash
git init                 # create a new repo (.git folder appears)
git status               # see what Git knows about your files
```

`git status` is the command you will run most — it shows what is modified, staged, or untracked.

### Configuring your identity (one-time)
Git stamps every commit with a name and email. Set them once per machine:

```bash
git config --global user.name  "Ada Lovelace"
git config --global user.email "ada@example.com"
```

### The core loop: stage and commit
```bash
git add Program.cs          # stage one file
git add .                    # stage everything changed
git commit -m "Add greeting logic"
```

Write commit messages in the **imperative mood** ("Add", "Fix", "Update") and describe *why*, not just *what*. Good history is a gift to your future self and your reviewer.

### Connecting to a remote and pushing
A **remote** is a copy of the repo hosted elsewhere (e.g. GitHub) so others — and the trainer reviewing your work — can see it. After creating an empty repo on GitHub:

```bash
git remote add origin https://github.com/you/your-repo.git
git branch -M main                 # name the default branch 'main'
git push -u origin main            # upload commits; -u links local to remote
```

After the first `push -u`, later pushes are just:

```bash
git push
```

### Keeping junk out with .gitignore
Build outputs, secrets, and editor files do not belong in the repo. A `.gitignore` file lists patterns Git should never track. For a .NET project:

```gitignore
bin/
obj/
.vs/
*.user
```

Add `.gitignore` *before* your first commit so the junk is never tracked in the first place.

## Code Example (When Relevant)
A complete first-repo session, start to pushed:

```bash
mkdir FirstName-Lastname && cd FirstName-Lastname
git init
echo "bin/"  >  .gitignore
echo "# FirstName Lastname — Training"  >  README.md
git add .
git commit -m "Initialize repo with README and gitignore"
git remote add origin https://github.com/you/FirstName-Lastname.git
git branch -M main
git push -u origin main
```

## Summary
- **Git** is distributed version control: full history in every clone; safe undo and collaboration.
- A file moves **working directory -> staging area (git add) -> repository (git commit)**.
- The daily loop is **edit -> `git add` -> `git commit -m` -> `git push`**; `git status` shows where you stand.
- A **remote** (`origin`) hosts the repo on GitHub; `git push -u origin main` publishes it the first time.
- A `.gitignore` keeps build output and secrets (`bin/`, `obj/`, `.vs/`) out of history — add it first.

## Additional Resources
- [Git Handbook — GitHub](https://docs.github.com/en/get-started/using-git/about-git)
- [Recording changes to the repository — Pro Git](https://git-scm.com/book/en/v2/Git-Basics-Recording-Changes-to-the-Repository)
- [gitignore reference](https://git-scm.com/docs/gitignore)
