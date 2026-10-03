# Repository Instructions

## Source of truth

Before responding to any task or changing any file:

1. Read `docs/assessment.md`.
2. Identify which assessment requirement the task supports.
3. Check the proposed work against `docs/architecture.md`.
4. Do not add scope that conflicts with the assessment or the three-day delivery
   priority without calling it out first.

After making a change, confirm that required claimant, claims officer, and
manager behaviour still matches `docs/assessment.md`.

## Priorities

- Backend correctness and tests come before frontend polish.
- Keep the solution small and easy to explain in the walkthrough.
- Use the modular-monolith boundaries in `docs/architecture.md`.
- Use REST/HTTP for the MVP. Kafka is deferred unless explicitly requested.
- Use PostgreSQL through Docker Compose. Do not replace it with an in-memory
  production data store.
- Do not implement production authentication. Use the documented demo identity
  approach unless explicitly asked to reconsider it.
- Avoid speculative abstractions and infrastructure.

## AI working journal

Update `docs/ai_working_journal.md` when a major decision is made or changed.
Write in first person with short, casual sentences. Record what AI suggested,
what I accepted or challenged, and why. Do not turn the journal into a polished
end-of-project summary. Small mechanical changes do not need an entry.

## Documentation

`docs/assessment.md` always takes priority over the implementation and
`docs/architecture.md`.

If implementation differs from the architecture, compare it with the assessment
first. If it conflicts with a requirement, fix the implementation. Do not change
the architecture just to justify the conflict.

If the difference is intentional and still meets the assessment, update the
architecture and journal in the same change. Keep all commands and startup
instructions repeatable for someone cloning the repository.
