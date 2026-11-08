# doc/

Raw project docs — GDD, specs, design notes, in any shape. Rough is fine. `/init-unity` reads
everything here as the authoritative source for game design.

`doc/context/` is the exception: it's **generated** by `/init-unity` and refreshed by
`/update-unity` — structured knowledge Claude reads at the start of a task. Review its diffs; don't
treat it as a source.
