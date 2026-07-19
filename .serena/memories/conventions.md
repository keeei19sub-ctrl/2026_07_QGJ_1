# Conventions
- `.agents/AGENTS.md` requires staging changes with `git add .` and committing after each change.
- All UI sizing/position specifications must use percentages.
- Existing C# style generally uses one class per file, PascalCase types/methods/properties, camelCase locals/private fields, and Unity lifecycle methods (`Awake`, `Start`, `Update`).
- Preserve Unity `.meta` files and their GUIDs when moving or resolving add/add conflicts.
- Do not hand-normalize whitespace throughout Unity-generated YAML/assets; keep changes narrowly scoped.