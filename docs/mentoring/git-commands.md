# Git Commands

## Show the Current Branch

```powershell
git branch --show-current
```

- `git` — the version control system.
- `branch` — works with branches.
- `--show-current` — prints only the active branch name.

## Show Changed Files

```powershell
git status --short
```

- `status` — shows the state of the working tree and staging area.
- `--short` — uses a compact output format.

Common status markers:

- `M` — a tracked file was modified.
- `??` — a new file is not tracked by Git yet.
- The first column represents the staging area; the second represents the working tree.

## Search Tracked Files

```powershell
git grep -n -E "net8\.0|dotnet-version: 8\.0"
```

- `grep` — searches files known to Git.
- `-n` — prints matching line numbers.
- `-E` — enables extended regular expressions.
- `|` inside the expression means “or.”
- `\.` means a literal period instead of the regular-expression wildcard.

## Stage Specific Files

```powershell
git add global.json src tests
```

- `add` — places selected changes in the staging area.
- The remaining arguments are the file and directories to stage.

Always inspect `git status --short` before staging. Avoid `git add .` without checking the working tree so unrelated changes are not included accidentally.

## Review Staged Changes

```powershell
git diff --staged
```

- `diff` — shows differences.
- `--staged` — shows only changes already staged for the next commit.

## Create a Commit

```powershell
git commit -m "chore: example message"
```

- `commit` — stores the staged snapshot in local history.
- `-m` — provides the commit message directly in the command.
- The quoted text is a short description of the change.

## Push a Branch to GitHub

```powershell
git push -u origin chore/LF-113-dotnet-10
```

- `push` — sends local commits to the remote repository.
- `-u` — connects the local branch to the remote branch so later pushes can use `git push`.
- `origin` — the conventional name of the remote repository.
- The final argument is the branch name to push.

## Configure the Commit Author for This Repository

```powershell
git config user.name "V-Serghei"
git config user.email "vistovschiiserghei@gmail.com"
```

Without `--global`, these values apply only to the current repository.
