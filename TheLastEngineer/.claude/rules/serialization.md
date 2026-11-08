---
description: "Unity serialization rules: SerializeField, renames, meta files, SerializeReference."
paths:
  - "**/*.cs"
---

# Serialization

Serialized data lives in scenes, prefabs and assets — a careless rename silently resets it.

- Expose fields to the Inspector with `[SerializeField] private`, not `public` fields.
- Never rename a serialized field without `[FormerlySerializedAs("oldName")]`. Keep the attribute
  until every scene and prefab has been re-saved.
- Never rename or move a `MonoBehaviour`/`ScriptableObject` class file without its `.meta` — the
  GUID is the reference. The class name must match the file name.
- Changing a serialized field's type loses its data; migrate it explicitly (new field +
  `FormerlySerializedAs` or an `ISerializationCallbackReceiver` step) and say so in the plan.
- Use `[SerializeReference]` only for real polymorphism, and leave a one-line comment saying why.
  Renaming or moving a type used there needs `[MovedFrom]`.
- Auto-properties don't serialize: use `[field: SerializeField]` or a backing field.
- Don't rely on serializing what Unity can't: dictionaries, interfaces (without
  `[SerializeReference]`), multidimensional arrays, `static`/`const`/`readonly` fields.
