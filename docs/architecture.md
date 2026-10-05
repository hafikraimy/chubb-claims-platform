# Claims Platform Architecture

## Goals

The application must demonstrate one complete claims journey:

1. A claimant reports a motor or property incident.
2. An officer picks up the claim, or a manager assigns it.
3. The officer reviews it, records an assessed loss amount, and may request information.
4. The claimant supplies the requested information.
5. The officer settles or rejects the claim.
6. The claimant can see the status, history, and final decision.
7. The manager can see team workload, performance, and outstanding exposure.

The implementation favours explicit business rules, reliable persistence,
authorization, and tests over infrastructure that the demo does not need.

## System overview

```text
One Angular application
  - claimant area
  - claims officer area
  - manager area
           |
           | REST/HTTP + JSON
           v
One ASP.NET Core API (modular monolith)
  - Access
  - Claims
  - Work Management
           |
           | EF Core
           v
PostgreSQL in Docker
```

The backend is one deployable application. The modules are code boundaries,
not separately deployed services.

## Architecture decision: modular monolith

A modular monolith is a better fit than microservices because the domain is
small, the delivery window is short, and claim updates require consistent
transactions. Microservices would add deployment, networking, distributed
transactions, and eventual-consistency problems without a demonstrated scaling
or team-ownership need.

The API is organised by feature:

```text
ClaimsPlatform.Api
  Access/             controllers, demo identity, authorization policies
  Claims/             controllers, application services, domain workflow
  WorkManagement/     controllers, work and reporting services
  Infrastructure/     EF Core, PostgreSQL, seeding
  Common/             error handling and shared API concerns
```

Controllers are the HTTP boundary and stay thin: they apply authorization,
translate service outcomes to status codes, and return DTOs. Focused
application services orchestrate use cases and query EF Core. Domain rules stay
on domain entities. EF Core is used directly as the repository and unit of work;
an additional generic repository, MediatR, event sourcing, and a CQRS framework
are unnecessary for this scope.

## Roles and authorization

The three roles are deliberately distinct:

| Capability | Claimant | Claims Officer | Manager |
| --- | --- | --- | --- |
| Submit a claim | Own | No | No |
| View claims | Own only | Assigned claims and eligible queue | Team and market |
| Respond to an information request | Own claim | No | No |
| Pick up an unassigned claim | No | Yes | No |
| Assess, record assessed loss, request information | No | Assigned claim | No |
| Settle or reject | No | Assigned claim | No |
| Assign or reassign an officer | No | No | Yes |
| View workload and performance | No | Own workload and read-only team summary | Team workload |
| View outstanding exposure | No | Own claims | Team/market dashboard |

Managers do not have a personal claim workload and are not a second approval
layer. An officer makes the settlement or rejection decision. A production
system could add manager approval above a settlement threshold, but the brief
does not define that policy and it would expand the state machine significantly.
Officers can see a read-only team summary because the assessment explicitly
asks claims staff to see their team's workload and performance. Only managers
can use that view to assign or reassign work.

The API enforces all authorization. Angular route guards and hidden controls
only improve the user experience; they are not security boundaries.

## Demo identity instead of full authentication

The database is seeded with a claimant, claims officers, and a manager. The
Angular application provides a demo-user selector and sends the selected user
ID in an `X-Demo-User` header. ASP.NET Core middleware resolves the seeded user
and creates a `ClaimsPrincipal`, allowing normal policy-based authorization.

This is identity simulation, not production authentication. The trusted header
would be replaced by an OIDC/JWT identity provider in a real deployment.
Passwords, registration, token issuing, refresh tokens, and account management
are out of scope.

## Core domain model

### Team

A team belongs to a market. It has one manager and one or more claims officers.

### User

A demo identity with a name, role, market, and optional team. A claimant owns
claims. An officer may be assigned claims. A manager supervises a team.

### Claim

The aggregate root. It stores the current workflow state and owns the rules for
valid transitions. A claim belongs to one claimant and may have one current
officer. Motor and property claims share the same model, identified by a claim
type, because the assessment does not define different workflows for them.

The claim records:

- Reference number, type, policy number, market, and currency.
- Incident date, location, and description.
- Claimant-reported loss amount and the officer-assessed loss amount.
- Current status and assigned officer.
- Final decision reason and settlement amount where applicable.
- Submission and update timestamps.
- A concurrency version.

### Information request

An officer's question and the claimant's response. There may be multiple
requests over a claim's lifetime, but only one open request is allowed at a
time for the MVP. Its purpose is to pause a decision when the officer is missing
a specific fact. For example, an officer could ask the claimant to confirm the
incident location or provide a police report reference.

This is structured Q&A, not a conversation thread. Each request has one question
and one answer. Answered requests remain as read-only history. If the answer is
not enough, the officer creates another request after reviewing it.

### Final decision

The final decision is stored on the claim instead of in a separate decision
entity. The terminal status records whether it was settled or rejected, while
the claim stores the reason and settlement amount where applicable. The
assigned officer is the deciding officer, and the final update timestamp is the
decision time. Terminal claims cannot be reassigned or changed.

### Claim history

An append-only audit trail for status, assignment, assessed-loss, and information
events. Each entry records the acting user, timestamp, event type, and a short
description. This avoids separate assignment and assessed-loss history tables
while retaining a useful walkthrough audit trail.

The **reported loss amount** is the claimant's initial estimate. The **assessed
loss amount** is the officer's estimate after reviewing the claim. It can change
when more information arrives. Neither value is a payment. The assessed loss is
used as a simple estimate of outstanding liability because detailed policy and
coverage calculations are outside the assessment.

## Claim workflow

```text
Submitted
    | officer pickup or manager assignment
    v
InReview <--------- claimant responds ---------+
    |                                         |
    +-- request information --> AwaitingInfo --+
    |
    +-- settle --------------------------> Settled
    |
    +-- reject --------------------------> Rejected
```

Rules:

- Submission creates a claim in `Submitted`.
- An officer pickup is atomic: only one officer can win a concurrent pickup.
- Manager assignment also moves a submitted claim to `InReview`.
- Reassignment changes the officer without changing an active workflow state.
- Only the assigned officer can assess, request information, record the assessed
  loss, settle, or reject.
- Requesting information moves `InReview` to `AwaitingInfo`.
- The owning claimant can answer the open request; this returns the claim to
  `InReview`.
- Only an `InReview` claim can be settled or rejected.
- Settlement requires a positive settlement amount and records the decision.
- `Settled` and `Rejected` are terminal states.
- Every operation appends a claim-history entry in the same transaction.

Invalid transitions return an RFC 7807 problem response with HTTP `409
Conflict`. Missing resources return `404`, validation failures return `400`,
and unauthorized access returns `401` or `403` as appropriate.

## PostgreSQL data model

PostgreSQL runs as a local Docker container. This is a real PostgreSQL instance,
but it requires no hosted database or shared credentials. EF Core migrations
create the schema, and development startup seeds deterministic demo identities
and their team. Claims are created through the UI rather than seeded. The
panel can clone the repository and create its own database with Docker Compose.

For the evaluator path, Docker Compose also builds the ASP.NET Core API and the
Angular application. Angular is compiled in a Node build stage and served by
an unprivileged nginx container, which proxies `/api` to the API over the
Compose network. The API is published in a .NET SDK build stage and runs in the
smaller ASP.NET Core runtime image. PostgreSQL remains the only stateful
container. This packaging does not change the modular-monolith boundaries;
native development startup remains available for faster iteration.

The minimal schema is:

| Table | Important relationships and data |
| --- | --- |
| `teams` | Market and manager |
| `users` | Role, market, optional team |
| `claims` | Claimant, current officer, incident, amounts, status, final decision, version |
| `information_requests` | Claim, requesting officer, question, response |
| `claim_history` | Append-only claim audit entries and acting user |

Use UUID primary keys, `timestamptz` timestamps, `numeric(18,2)` monetary
values, and three-character ISO currency codes. Store statuses as readable
strings constrained by the application and database. Monetary amounts must
always be interpreted with their currency.

The initial indexes are intentionally limited to actual access paths:

- Primary keys and the unique claim reference.
- Foreign-key indexes generated or verified in the migration.
- `claimant_id` for a claimant's claims.
- `assigned_officer_id` for officer workload.
- A partial index over submitted, unassigned claims may be added for the queue.

No standalone status or speculative reporting indexes are required for the
demo dataset. Further indexing should be driven by measured query plans.

The claim concurrency version is an optimistic-concurrency token. Conflicting
updates, especially simultaneous officer pickup, return `409 Conflict` instead
of silently overwriting data.

## REST API responsibilities

REST is used for all implemented commands and queries because users need an
immediate authorization, validation, and persistence result.

Representative endpoints:

```text
GET  /api/me

POST /api/claims
GET  /api/claims
GET  /api/claims/{claimId}

GET  /api/work/queue
GET  /api/work/my-claims
GET  /api/work/team-summary
POST /api/claims/{claimId}/assign-to-me
POST /api/claims/{claimId}/assignments
POST /api/claims/{claimId}/information-requests
POST /api/claims/{claimId}/information-requests/{requestId}/response
POST /api/claims/{claimId}/assessed-loss
POST /api/claims/{claimId}/settle
POST /api/claims/{claimId}/reject

GET  /api/manager/dashboard
```

Endpoints return DTOs rather than EF entities. ASP.NET Core's OpenAPI document
is the source of truth for the frontend contract. The Angular application uses
a small typed API service; client generation is optional if setting it up costs
more time than it saves.

Claim detail is one resource at `GET /api/claims/{claimId}` and uses the same
response contract for every role. Authorization filters the resource by the
current identity: a claimant can see their own claims, an officer can see their
assigned claims and eligible unassigned queue claims, and a manager can see
unassigned claims in their market plus claims assigned to officers in their
team. Role-specific capabilities such as responding, assessing, deciding, and
assigning remain separate command endpoints.

Claims processing is asynchronous in the business sense, not as one long HTTP
request. Each user action is a short synchronous request that persists a new
state. Later users load or refresh that state through REST. Manual refresh or
light polling is sufficient; WebSockets are out of scope.

## Kafka decision

Kafka is deferred from the MVP. There is currently no downstream service that
needs claim events, so using Kafka for user commands would add latency and
failure modes without improving the workflow.

If time remains after the core application is complete, Kafka can publish facts
such as `ClaimSubmitted`, `InformationRequested`, `ClaimSettled`, and
`ClaimRejected` for notifications or analytics. A reliable implementation must
use a transactional outbox so a claim change and its pending event are committed
together. A direct "save, then publish" implementation is not acceptable
because either operation could fail independently.

This extension is deliberately not a prerequisite for the working application.

## Angular application

All three roles use one Angular application and one deployment. Role-specific
areas are routes within the same application:

```text
/select-user
/claimant/claims
/claimant/claims/new
/claimant/claims/:id
/officer/queue
/officer/my-work
/officer/claims/:id
/manager
/manager/claims/:id
```

The demo-user selector stores the selected user locally. An HTTP interceptor
adds the demo-user header. Route guards direct the user to the correct area.
Shared components handle claim summaries, status display, history, loading,
and API errors.

Angular services and normal observable or signal state are sufficient. NgRx is
not justified for this application.

The frontend priority is a thin path through each role rather than a polished
design system:

- Claimant: create, list, view, and answer a request in a text form.
- Officer: queue, own workload, claim assessment, request form, and final decision.
- Manager: team dashboard, assignment, and read-only claim detail.

The information request must appear in the UI because providing additional
information is an explicit claimant requirement. The officer sees a short
question form on the claim. The claimant sees the open question on their claim
detail page and answers it with text.

Both users see an Additional Information section on the claim detail page. It
lists previous questions and answers with their timestamps. The current open
question appears first. After the claimant answers, the officer sees the answer
by reloading the claim detail. The claim returns to `InReview`. File attachments,
answer editing, and a general chat system are outside the MVP.

## Workload, performance, and liability

These figures are direct PostgreSQL/EF Core projections; no reporting database
or cache is needed.

### Workload

Open claims are `Submitted`, `InReview`, or `AwaitingInfo`. Report:

- Unassigned open claims.
- Open claims per officer and by status.
- Claims awaiting claimant information.
- Average and oldest open-claim age.

### Performance

For a selected period, initially the last 30 days, report:

- Settlement and rejection decisions per officer.
- Average time from submission to decision.
- Total decisions completed.

Decision attribution uses the assigned officer on terminal claims. This is safe
for the MVP because only the assigned officer can decide a claim and terminal
claims cannot be reassigned. Approval rate is not treated as a quality metric
because claim complexity and outcome correctness are not available.

### Outstanding liability

For each open claim, exposure is the officer-assessed loss amount. It falls back
to the claimant-reported loss amount when the officer has not entered an
assessment:

```text
outstanding exposure = sum(coalesce(assessed loss, reported loss))
                       for open claims
```

Settled and rejected claims are excluded. Results are grouped by market and
currency. Different currencies are never summed without an exchange-rate
source, which is outside this assessment.

## Testing priorities

Backend tests have priority over frontend test breadth:

1. Unit tests for every allowed and rejected state transition.
2. Authorization tests for claimant ownership, officer assignment, and manager
   team boundaries.
3. An integration test for concurrent officer pickup.
4. Integration tests for the main submit-to-decision path using PostgreSQL.
5. Query tests for workload and outstanding-exposure calculations.

The happy path alone is not enough; invalid transitions and cross-user data
access are important evidence of backend quality.

## Deliberately not built

- Microservices or independently deployed workers.
- Production authentication or user administration.
- Kafka in the initial implementation.
- Policy lookup or coverage validation.
- File uploads, object storage, OCR, or antivirus scanning.
- Payment processing or partial settlements.
- Manager approval thresholds or configurable workflow engines.
- Email, SMS, push notifications, or WebSockets.
- Fraud detection or automated claim decisions.
- Currency conversion.
- Redis, Elasticsearch, a data warehouse, or dashboard caching.
- Event sourcing, a generic repository, MediatR, or a CQRS framework.
- Kubernetes or cloud deployment.
- A large design system or complex Angular state management.

These are conscious scope decisions, not assumed production limitations.

## Assessment requirement coverage

| Assessment requirement | Design coverage |
| --- | --- |
| Report an incident | Claimant creates a motor or property claim through REST |
| Track a claim | Claim detail exposes current status, history, and assignment |
| Provide additional information | Officer request and claimant response workflow |
| Receive decisions | Settlement or rejection is visible on the claim |
| Pick up incoming claims | Atomic officer pickup from the unassigned queue |
| Review and assess | Officer detail, assessed-loss update, and information request |
| Progress to settlement or rejection | Explicit guarded workflow commands |
| Team workload and performance | Read-only staff summary plus manager dashboard |
| Outstanding liability exposure | Open-claim assessed-loss calculation grouped by currency |
| One Angular application | Role-based routes in a single Angular codebase |
| Backend choice | ASP.NET Core/.NET with EF Core |
| Communication decision | REST for MVP; Kafka extension documented and deferred |
| Database decision | PostgreSQL provided through Docker Compose |
| Async claims processing | Persisted workflow across short HTTP interactions |
| Frontend/backend contract | Backend-owned OpenAPI document and typed Angular service |
| Local working application | Docker-based database, migrations, seed data, documented startup |

## Delivery priority

The order of work is:

1. PostgreSQL schema, workflow, REST endpoints, and authorization.
2. Backend unit and integration tests.
3. Workload, performance, and liability queries.
4. Minimum Angular flows for all three roles.
5. Startup documentation, demo data, and walkthrough preparation.
6. Kafka only if everything above is complete and stable.
