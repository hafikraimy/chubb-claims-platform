# AI Working Journal

This is a running log of my main AI-assisted decisions. Entries are in order.

## Started with the backend architecture

I asked AI to read the assessment and inspect the repo first. I did not want
code before agreeing on the architecture.

The first proposal was useful but too large. It included Kafka, more
tables, and more workflow steps. I challenged whether those parts were needed
and possible in the time available.

## Chose a modular monolith

AI suggested one modular API instead of microservices. I accepted this. The workflow needs simple database transactions. Microservices would add more
failure cases and setup work.

Access, Claims, and Work Management will be feature areas. They will stay in one
ASP.NET Core deployment.

## Cut the design down

I asked if the first plan was realistic. It was not realistic in full. I chose one complete workflow over more infrastructure. It covers submission,
assignment, review, extra information, settlement, and rejection.

I left out partial settlements, payment integration, and document uploads. The
brief does not require them.

## Simplified the claim states

AI first suggested separate `Approved` and `Settled` states. I removed the extra
approval step. The brief does not define a payment process. It only asks for settlement or
rejection.

The states are `Submitted`, `InReview`, `AwaitingInfo`, `Settled`, and
`Rejected`. Settled and Rejected are terminal.

## Clarified the manager role

I challenged the first role table. It gave managers a personal workload even
though they could not pick up claims. Managers will manage the team. They can assign claims and view team metrics.
They do not own a personal claim workload.

Officers can still see a read-only team summary. The brief asks claims staff to
see team workload and performance. AI considered additional manager approval for settlements. I left it out. The brief does not define thresholds or escalation rules.

## Deferred Kafka

AI first proposed Kafka events and a transactional outbox. I questioned whether
the demo needed them. Users need immediate validation and results. REST fits every current operation.

I chose not to add Kafka yet. There is no real downstream system in this demo.
Kafka would add setup and failure handling.

## Chose PostgreSQL in Docker

I asked how the interviewer panel could run the database. A PostgreSQL container keeps the setup local and simple.

I will provide Docker Compose, EF Core migrations, and seed data. The panel will
not need my database or cloud credentials.

Development startup may apply migrations and seed data. Production deployment
would handle this separately.

## Kept the schema and indexes small

The first proposal had more tables and indexes. I challenged whether they were
needed for this build.

I kept teams, users, claims, information requests, decisions, and claim history.
One history table covers the main audit events.

Indexes will match real access paths. I will keep primary, unique, foreign-key,
claimant-list, and officer-workload indexes.

I may add an unassigned-queue index. I will not add speculative indexes.

## Chose demo users

AI suggested seeded users and an `X-Demo-User` header. I accepted this.

It demonstrates role rules without building login and token flows. The API will
still enforce ownership, assignments, and team boundaries.

This is identity simulation. Angular route guards are not the security layer.

## Kept one Angular application

The brief requires one Angular app for claimant and staff users. I will use
role-based routes and one shared API layer.

I am not adding NgRx. The UI only needs a thin flow for each role.

Backend behaviour and tests remain the priority. The brief allows a partial
frontend.

## Kept information requests visible in the UI

The brief says claimants must provide more information when asked. It also says both user types use the Angular app. I decided to show this flow in the UI.

This will be simple Q&A, not chat. Only one question can be open at a time. The claimant answers it on the claim detail page.

Answered questions stay visible as history. The officer can review the answer on the same claim page. I left out attachments and answer editing.

## Kept manager metrics explainable

AI suggested direct PostgreSQL queries for the dashboard. I accepted this.

Workload shows open claim counts and age. Performance shows completed decisions
and average decision time.

The brief needs outstanding liability. AI first called the officer's estimate a
case reserve. I found that term confusing.

I changed it to assessed loss amount. It falls back to the claimant's reported
loss until the officer enters an assessment.

Results stay grouped by currency. I will not create a misleading combined total.

I will not use approval rate as a quality score. The demo lacks enough context.

## Challenged the two composite claim indexes

AI suggested indexes on `(claimant_id, submitted_at)` and
`(assigned_officer_id, status)`. I challenged whether either pair is justified. A claimant should only have a small claim history, and an officer should not have thousands of active claims. Sorting or filtering those small result sets should be cheap after finding the relevant user.

I also pointed out that indexes are not free. Inserts must maintain every index, and including status adds work whenever a claim changes state.

AI suggested simpler indexes on `claimant_id` and `assigned_officer_id`. They
still support ownership, foreign-key lookups, and each user's claim list. I am recording the challenge before changing the mapping or architecture. I will only add composite indexes later if the real queries and measured plans justify them.

## Simplified team management

I questioned the manager relationship because `WithMany` was confusing. I chose a simple one-to-one rule, one team has one manager, and one manager manages only one team.

## Kept final decisions on the claim

I challenged the separate decision model because it duplicated claim status and claim decision status. I kept the decision reason and settlement amount on the claim.

## Deferred integration tests

AI suggested temporary PostgreSQL integration tests for the claimant API. I deferred them because time is running short. I manually verified the API against the Docker PostgreSQL database and will prioritise the remaining user workflow first.

## Unified claim detail

I questioned why claimant, officer, and manager detail views needed different
endpoints when they return the same claim data. AI suggested treating claim
detail as one resource and keeping role differences in authorization and
commands. I accepted this.

`GET /api/claims/{claimId}` is shared. Claimants see their own claims, officers
see assigned and eligible queue claims, and managers see their market's
unassigned claims and claims assigned to their team. Assignment and workflow
actions stay role-specific.

## Added manager assignment

I implemented manager assignment after agreeing that detail remains shared.
The API checks that the claim is in the manager's scope and that the selected
officer belongs to the manager's team and market. The claim domain handles the
difference between assignment and reassignment and records the manager as the
actor.

## Made manager assignment usable

I added one manager dashboard query instead of separate claim and officer-list
endpoints. It returns open claims in the manager's scope, team officers with
workload counts, and outstanding exposure grouped by currency. This gives the
manager screen the data needed to assign work without expanding the API.

I kept historical performance metrics for a later slice so this change stays
focused on assignment and current workload.
