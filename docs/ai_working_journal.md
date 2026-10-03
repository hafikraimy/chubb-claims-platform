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
