# Suggested commands (PowerShell)
- Run/import project headlessly: `& 'C:\Program Files\Unity\Hub\Editor\6000.5.2f1\Editor\Unity.exe' -batchmode -nographics -quit -projectPath $PWD -logFile -`
- Fast tracked-file search: `rg -n '<pattern>' Assets ProjectSettings Packages`.
- List repository files: `rg --files`.
- Inspect worktree: `git status --short --branch`.
- Find unresolved merge entries: `git diff --name-only --diff-filter=U`.
- Find conflict markers: `git grep -n -E '^(<<<<<<<|=======|>>>>>>>)' -- '*.cs' '*.asset' '*.meta'`.
- On Windows, use PowerShell `Get-Content`, `Get-ChildItem`, and `Select-String` when shell-native inspection is preferable.