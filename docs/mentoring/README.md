# LedgerFlow Mentoring Notes

This directory is our shared learning reference. It records commands we have already used, why we used them, and explanations of important code concepts.

## How We Work

1. Before introducing a new command, explain which problem it solves.
2. If the command is not documented here yet, explain every part and add it to these notes.
3. If the command is already documented, link to the relevant section instead of repeating the full explanation.
4. Explain the expected result and the signs of failure separately.
5. Do not change code blindly: locate the relevant code, read its dependencies, and describe the expected change first.
6. During code analysis, distinguish between:
   - **fact** — something directly visible in the code, command output, or documentation;
   - **assumption** — a possible explanation that still needs verification;
   - **decision** — the selected change and the reason for choosing it.

## Contents

- [.NET Commands](dotnet-commands.md)
- [Git Commands](git-commands.md)
- [Docker and PowerShell](docker-and-powershell.md)
- [How to Read Code](code-reading.md)
- [Work Log](work-log.md)

## Safety Rule

Before committing, pushing, deleting files, stopping containers, or changing the database, inspect the current state and explicitly describe the consequences.
