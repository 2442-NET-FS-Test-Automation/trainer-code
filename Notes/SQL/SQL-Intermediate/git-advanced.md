# Git Advanced — Reinforcement (gitignore, branches, PRs, Git Flow)

> **This is a short hands-on reinforcement activity, not a full concept note.** It is a non-SQL rider for Thursday: you practice intermediate Git on the repos you already have (`sql-training`, `core-csharp-kata`) while you version this week's `.sql` scripts. ~10 minutes of reading, then do it in your repo.

## Learning Objectives

- Keep junk out of a repo with a `.gitignore`.
- Move between lines of work with `git checkout` / `git switch` and feature branches.
- Open and merge a **pull request** (a reviewed merge), and resolve a simple conflict.
- Recognize the **Git Flow** branching model and when teams use it.

## Why This Matters

You have already made commits and opened a first PR (Week 1). This week's SQL work — a growing `LibraryDB.sql` build script and query files — is a perfect, low-stakes place to practice the *next* layer of Git: branching for each piece of work, ignoring generated files, and merging through review. These are the everyday mechanics of working on a team, and the exact workflow your multi-week team project will run on. Practice them now on throwaway `.sql` files so they're muscle memory when the stakes are higher.

## The Activity

### 1. `.gitignore` — don't commit junk

A `.gitignore` lists paths Git should **not** track. For a SQL/.NET repo you never want build output, local tool files, or secrets in history:

```gitignore
# build output
bin/
obj/
# local SQL tool files
*.bak
*.mdf
*.ldf
# editor / OS noise
.vs/
.vscode/
*.user
.DS_Store
```

- Add `.gitignore` at the repo root **before** your first messy commit — it's far easier than removing tracked junk later.
- Already tracked a file you meant to ignore? `git rm --cached <file>` stops tracking it (keeps it on disk), then commit.

### 2. Feature branches + checkout/switch

Do each unit of work on its own **branch**, never directly on `main`:

```bash
git switch -c feature/ddl-script   # create + switch to a new branch (modern)
# ...edit LibraryDB.sql, stage, commit...
git add LibraryDB.sql
git commit -m "Add Author and Member tables"
git switch main                    # back to main
git switch feature/ddl-script      # back to the work
```

- `git switch -c <name>` creates and moves to a branch (`git checkout -b <name>` is the older equivalent — both work).
- A branch is a cheap, isolated line of work; `main` stays clean and runnable.
- `git switch -` jumps back to the previous branch.

### 3. Push, pull request, merge

```bash
git push -u origin feature/ddl-script
```

Then on GitHub, open a **Pull Request** from your branch into `main`. A PR is a *reviewed merge*: a teammate looks at the diff, comments, approves, and then it merges. Keep PRs **small** (one logical change) so review is quick.

- **Pull / merge requests** are how change enters `main` on a team — not direct pushes.
- After a teammate's PR merges, sync your local copy: `git pull` (fetches and merges `origin/main`).
- A **merge conflict** happens when two branches change the same lines. Git marks them with `<<<<<<<`, `=======`, `>>>>>>>`; edit the file to the version you want, remove the markers, then `git add` + commit. For `.sql` files this is usually "keep both new tables" — straightforward.

### 4. Git Flow (recognize it)

**Git Flow** is a popular branching model that names long-lived and short-lived branches:

| Branch | Purpose |
|---|---|
| `main` | production-ready, released code |
| `develop` | integration branch for the next release |
| `feature/*` | one branch per feature, merged into `develop` |
| `release/*` | stabilize a release before it hits `main` |
| `hotfix/*` | urgent fix branched off `main` |

You don't need full Git Flow for this week's solo SQL practice — a simple `feature/* → main` flow is plenty. Just **recognize** the model: many teams use it (or a lighter "GitHub Flow" = `feature/* → main` with PRs), and interviewers may ask you to describe one.

## Do It Now (checklist)

- [ ] Add a `.gitignore` to your `sql-training` repo (ignore `bin/`, `obj/`, `*.bak`, editor files).
- [ ] Create a `feature/...` branch for a chunk of this week's SQL work.
- [ ] Commit your `.sql` changes on the branch with clear messages.
- [ ] Push the branch and open a **PR** into `main`; have a peer glance at it before merging.
- [ ] `git pull` after merging to sync `main`.

## Common Mistakes

- **Committing `bin/`/`obj/` or `.bak` files.** Bloats history; add `.gitignore` first.
- **Working directly on `main`.** Branch per change; merge through a PR.
- **Giant PRs.** Hard to review — keep them to one logical change.
- **Panicking at a conflict.** Conflicts are normal: edit to the wanted result, delete the markers, `git add`, commit.

## Additional Resources

- [Ignoring files — GitHub Docs](https://docs.github.com/en/get-started/getting-started-with-git/ignoring-files)
- [About pull requests — GitHub Docs](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests)
- [git switch / branching basics — Git SCM](https://git-scm.com/book/en/v2/Git-Branching-Basic-Branching-and-Merging)
