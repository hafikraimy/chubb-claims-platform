# Demo walkthrough

## Start from a clean checkout

Run:

```bash
docker compose up --build
```

Open `http://localhost:4200`. Compose starts Angular, the ASP.NET Core API, and
PostgreSQL. The first startup applies migrations and creates four demo users and
one Malaysia team; it does not create claims.

If an old demo run is confusing the walkthrough, stop the app and run
`docker compose down --volumes` before starting it again. This deliberately
deletes the local demo database.

## Suggested end-to-end journey

1. Select **Hafiz Claimant** and create a Motor claim with:
   - Policy number: `POL-DEMO-001`
   - Currency: `MYR`
   - Incident date: today or an earlier date
   - Location: `Kuala Lumpur`
   - Description: `Rear bumper damaged at traffic lights.`
   - Reported loss: `2500`
2. Expect the new claim to appear in **My claims** with status **Submitted**.
   Open it and note the claim reference.
3. Switch user to **Aisha Manager**. Expect one open claim and MYR 2,500 of
   exposure. Assign it to **Ben Claims Officer**, then open the claim reference
   to see its read-only detail and history.
4. Switch to **Ben Claims Officer**. The app opens **My work** by default. Open
   the claim, record assessed loss `1900`, and request:
   `Please provide the police report reference.` Expect status **Awaiting info**.
5. Switch to **Hafiz Claimant**, open the claim, and answer `PR-12345`. Expect
   the claim to return to **In review**.
6. Switch to **Ben Claims Officer**, reload the detail, and verify the response
   and assessed loss. Settle it for `1700` with reason
   `Covered repair cost approved.` Expect status **Settled**.
7. Switch to **Hafiz Claimant** and verify the final reason and settlement.
   Switch to **Aisha Manager** and verify the terminal claim is no longer in
   open exposure and the decision is included in performance.

For the rejection path, submit a second claim, let Ben pick it up from the
unassigned queue, and reject it with a short reason.

## Demo data and field expectations

- The user selector is the demo authentication mechanism. It sends a fixed
  user ID in `X-Demo-User`; there are no passwords or production login flows.
- `POL-DEMO-001` is a dummy policy string. The MVP does not look up a policy or
  check coverage. Policy numbers accept up to 50 characters.
- Currency is a three-letter code such as `MYR`. The app groups exposure by
  currency and performs no foreign-exchange conversion.
- Reported loss is the claimant's estimate. Assessed loss is the officer's
  estimate. Settlement is the final approved amount.
- An information request is text only: question up to 1,000 characters and
  response up to 2,000. Only one request can be open on a claim at a time.
- Decision reasons accept up to 1,000 characters. Oversized API requests return
  validation errors before PostgreSQL is called.

## Known limitations

- Demo identity selection is not production authentication or authorization
  infrastructure.
- Policy verification, coverage rules, fraud checks, attachments, payments,
  notifications, email, and general chat are outside the assessment MVP.
- Information responses cannot be edited, and only one request can be open at a time.
- Liability is a direct operational projection, not an actuarial reserve model.
- Reporting is fixed to the manager's team and the last 30 days; there are no
  configurable reports or exports.
