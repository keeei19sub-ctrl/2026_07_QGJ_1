# Task completion
- Confirm no unresolved entries: `git diff --name-only --diff-filter=U` returns nothing.
- Check edited/staged paths for conflict markers and patch whitespace errors (`git diff --cached --check -- <paths>`).
- For gameplay/script or Unity YAML changes, open/import with pinned Unity `6000.5.2f1` in batch mode; require a normal process exit and verify `Library/ScriptAssemblies/Assembly-CSharp.dll` is generated for a fresh worktree.
- Remove only test-generated unstaged changes after inspection; ensure `git status --short` contains only intended changes.
- Follow repository rule to stage and commit each completed change.