# CHUBB APAC Engineering

## Take-Home Assessment: Fullstack Developer

**Time guidance:** 3 days

## Background

Chubb APAC processes motor and property claims across six markets. Today, the
process is fragmented: claimants submit by phone or email and wait with no
visibility into what is happening. Claims staff manage incoming work from
shared inboxes and spreadsheets, with no consolidated view of their workload.
Managers have no real-time picture of outstanding claims or liability exposure.

Your task is to build the platform.

### Claimant needs

Claimants need to be able to:

- Report an incident.
- Track their claim.
- Provide additional information when asked.
- Receive decisions.

### Claims staff needs

Claims staff need to be able to:

- Pick up incoming claims.
- Review and assess claims.
- Progress claims to settlement or rejection.
- See their team's workload and performance.

Both user types access the same Angular application.

## Technology

| Area | Requirement |
| --- | --- |
| Frontend | Angular (any version). Angular is required — it is the frontend framework across both platforms. |
| Backend | C#/.NET or Java/Spring Boot — your choice. Both are actively used across Chubb APAC engineering. |
| Communication | Our platforms use both REST/HTTP and Kafka. You decide what uses which, and you should be prepared to explain that decision. |

There are no other technology constraints. Database, caching, testing libraries,
API tooling, and frontend architecture are all your decisions.

## Approach

Start with the backend. The backend is the foundation of the platform and where
we expect the majority of your time to go. Extend to the frontend if time
permits.

A well-built backend with a partial frontend will outscore a rushed attempt at
both. The decision about how to allocate your time — and how you explain it —
is part of the assessment.

## What to Build

This brief is deliberately underspecified. You decide how to decompose the
problem, what the service boundaries are, what the data model looks like, how
the frontend is structured, and what to prioritise. The decisions you make —
and your ability to explain them — are a significant part of what is being
assessed.

### Questions to consider

- What are the core entities in this domain? How do they relate?
- Is the backend one service or more than one? What drives that decision?
- What operations need to be synchronous? What can be event-driven?
- How does the frontend serve two distinct user types from a single Angular
  codebase?
- What is the contract between frontend and backend, and who defines it?
- How do you handle the async nature of claims processing across both tiers?
- What does a claims officer need to see to manage their workload effectively?
- What does the system need to know about outstanding liability exposure?

You will not build everything. Decide what matters most and build that well.

## Deliverables

- Git repository with meaningful commit history showing your development
  process.
- Working application that can be started locally.
- AI working journal — a running log (committed alongside the code) of what you
  asked the AI, what you accepted, what you challenged, and what you overrode,
  with brief reasoning. It does not need to be polished.
- Any supporting documentation you feel is appropriate.
- 30–60 minute walkthrough with the hiring panel.

## Walkthrough Format

## Notes

This is a sprint-format assessment. We are not expecting a finished product —
we are evaluating how much you can build, and how well, when you work with AI
effectively.

This brief is deliberately underspecified. We expect you to decompose the
problem, make technology and architecture decisions, and design a solution.
Those decisions are part of what we are evaluating.

Prioritise ruthlessly. Decide what to build and what to leave out, and be
prepared to explain that prioritisation in the walkthrough.

AI is your primary working interface. We expect AI tooling to drive the bulk of
code generation. What we are evaluating is how you direct, challenge, and
override it. Document your process as you go. You sign off every line you
submit — the panel will probe anything you cannot defend.

The walkthrough is where your thinking is explored. Come prepared to explain
the approaches you took and why, the shortcuts you made under time pressure,
and what you would do differently or tackle next.

If you have questions about the assessment, contact the hiring panel at
[hiring panel contact].

Good luck.
