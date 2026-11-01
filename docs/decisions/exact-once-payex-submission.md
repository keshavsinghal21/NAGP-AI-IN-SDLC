# Decision: Exact-once PayEx submission

## Context

The production version described in `ops/README.md` is planned to add a database, a message topic, and an integration with PayEx. That same file says these parts are not in the code yet. The repository is still a demo application using an in-memory store, so there is no confirmed payment flow to rely on.

There is no evidence in this repository that PayEx supports idempotency keys, status queries, or callbacks. I cannot assume those features exist without confirmation from the provider.

Duplicate payment submission is a serious problem because it can cause a customer to be charged more than once for the same invoice. In a finance workflow, a duplicate submission is not just a technical issue; it can create incorrect payment records, customer complaints, operator investigation, and financial correction work.

This is particularly important because the incident evidence in `ops/incidents/INC-2041` shows that a PayEx submission call was introduced in a release and that failures were happening under load. The logs show `PayExClient.SubmitAsync(invoice=...)` and a mix of socket errors and request timeouts. That is evidence that external payment calls can fail in real ways, which makes retry and crash recovery important.

## Decision

My choice is:

**Option 1 — Idempotency key + deduplication table**

This decision is conditional on confirming the following PayEx requirements before implementation:

- PayEx supports stable idempotency keys across retries
- payment status can be queried or otherwise correlated by idempotency key or payment reference
- PayEx retry semantics are safe when the same key is submitted again

This does not mean PayEx already supports these features. They are conditions that must be confirmed before this design is implemented.
If those requirements are not met, we should revisit the decision and likely move toward a more durable outbox-based design.

## Options considered

### 1. Idempotency key + deduplication table

This approach uses a stable key for each payment attempt and records it in a local deduplication table. Before sending to PayEx, the app checks whether that key has already been accepted, in progress, or completed.

This helps prevent duplicates in the application itself, but it only works reliably if PayEx also handles duplicate requests safely. This is the simplest strong pattern if the provider supports it.

For this option:
- Duplicate prevention: strong, as long as the same key is reused and the provider treats it as the same payment
- Crash recovery: reasonable, if the app records the key and state before the external call and can retry safely
- Retry behavior: good if the same key is reused and provider deduplication works
- Concurrent submissions: manageable with a database uniqueness constraint or a lock on the payment key
- Operational complexity: moderate
- Development/build cost: low to medium
- Long-term operational cost: medium

### 2. Transactional outbox + worker

This approach records the payment intent in the same database transaction as the invoice state change. Then a background worker reads the outbox and submits to PayEx. The outbox is durable, so the app can recover from crashes without losing the work item.

This is strong for crash recovery and system resilience. It still does not guarantee exactly-once payment submission by itself, because the payment provider still has to tolerate retries or server-side deduplication.

For this option:

- Duplicate prevention: good, because the payment intent is recorded and processed once per outbox row
- Crash recovery: strong, because the intent survives on disk before the external call
- Retry behavior: strong, because a worker can retry the outbox row
- Concurrent submissions: strong, because the database transaction makes the decision durable before processing
- Operational complexity: high
- Development/build cost: medium to high
- Long-term operational cost: medium to high

### 3. Optimistic submission + nightly reconciliation

This approach submits quickly and then later compares local records with PayEx records. It assumes the system can tolerate a later correction if the payment state was not recorded cleanly.

This is the cheapest approach to build, but it is the weakest for correctness. It handles the happy path well, but it is not a strong exact-once design.

For this option:

- Duplicate prevention: weak, because it relies on later detection rather than prevention
- Crash recovery: weak to moderate, depending on whether the app has records to reconcile
- Retry behavior: mostly manual or reconciliation-driven
- Concurrent submissions: poor, because two submissions can happen before discovery
- Operational complexity: medium
- Development/build cost: low
- Long-term operational cost: high

## Why Option 1

I would start with Option 1 because it is the smallest design that can provide strong protection against duplicate submission, assuming PayEx supports the required behavior.

It has a lower infrastructure cost than an outbox + worker. We do not need to introduce a new background worker or a full async processing model just to start handling a payment. We can create a durable payment-submission table and use a stable key for each invoice/payment attempt.

It is easier to implement and test. The team can add a record for the payment intent, enforce uniqueness on the invoice/payment key, and verify retry behavior under concurrent requests. This fits the current repository shape better than introducing a broker and worker before we have the production architecture in place.

This also fits the current stage of the project. The database, broker, and PayEx integration are still planned, not implemented. I would rather verify the provider contract before introducing a complete worker system.

This does not mean Option 2 is a bad design. It is a strong future alternative. If we later confirm that PayEx is not safe to retry with the same key, or if the workflow becomes more asynchronous and operationally complex, an outbox + worker design becomes more attractive.

## Challenge: Why not Option 2?

The strongest case for transactional outbox + worker is that it records the payment intent in a durable database transaction before any external system is touched. That matters when the application crashes. If the app dies after the database commit but before PayEx receives the request, the work is still there. The worker can continue processing later. The database becomes the source of truth for “what still needs to happen.”

It also gives stronger async processing. The approval flow and the payment call are separated. This makes it easier to recover from timeouts and partial failures. Auditability is better because the outbox row records the event that was intended to be sent. That is useful when tracing what happened after a late-night outage or an external provider timeout.

Operationally, it is more resilient. When multiple instances are running, or when the app is restarted, the worker can pick up the work again without relying on a single request thread to complete the payment flow. That is important if the system is distributed or if the app is restarted during peak volume.

Why it was not selected as the first implementation:

- it adds more infrastructure, more services, and more monitoring
- it is a bigger change than the team needs while it is still clarifying the PayEx contract
- it does not remove the need for PayEx-side idempotency or status lookup
- it is a stronger long-term design, but not the simplest first step

In other words, Option 2 is a good future architecture, but it is not the most efficient first proof point for this project.

## 3 a.m. last-day-of-month failure

Scenario:

At 3:00 a.m. on the last day of the month, a payment is submitted to PayEx. PayEx accepts/processes it. Before the application receives or records the response, the application crashes.

This is the exact case where the system has to answer: did PayEx already take the payment, or did the app lose the result?

### Option 1 — Idempotency key + deduplication table

- Local state: the app may have a dedup record such as “payment key created” or “submission in progress.” It may also have no final confirmation row because the response was lost in the crash.
- Possible PayEx state: PayEx may already have accepted and processed the payment, or it may have failed and not processed it. The app cannot know without a provider status lookup.
- Recovery after restart: the app can retry using the same idempotency key if the key is still in a safe state, or it can query PayEx by key/reference if that capability exists.
- Is retry safe?: safe only if PayEx treats the same key as the same payment and rejects or returns the same status for duplicates
- Possibility of double charging: yes, if the provider does not deduplicate requests with the same key or if the app submits a new key after restart
- Need for manual intervention: likely if there is no reliable provider query or if the app cannot prove the payment state
- Evidence required: local dedup record, payment key, PayEx status lookup or reference, provider audit logs, and the invoice timeline

This option can recover well only if we know the provider contract. Without that, we are still operating on assumptions.

### Option 2 — Transactional outbox + worker

- Local state: the payment intent is still in the durable database. The outbox row remains and can be reprocessed after restart.
- Possible PayEx state: PayEx may already have processed the payment before the app wrote the response.
- Recovery after restart: the worker resumes and processes the pending outbox row. It can retry or query the provider if needed.
- Is retry safe?: safer than a blind retry, because the work item is not lost, but still not automatically safe unless PayEx supports idempotent requests or lookup
- Possibility of double charging: lower than optimistic submission, but still possible if the provider does not deduplicate requests correctly
- Need for manual intervention: possibly, but less likely if the worker and status-check procedure are designed well
- Evidence required: outbox row, worker logs, payment request identifier, PayEx status or reference data, and the database state after restart

This option is very good for crash recovery, but it still depends on provider semantics.

### Option 3 — Optimistic submission + nightly reconciliation

- Local state: the app may have approved the invoice or marked the payment as submitted, but it may not have a reliable record of the actual PayEx result.
- Possible PayEx state: PayEx may have processed the payment, rejected it, or accepted it but the app never received the response.
- Recovery after restart: the app may retry or start a reconciliation job later. The night process compares local records with PayEx records.
- Is retry safe?: usually not safe by default. A blind retry can create duplicate charges.
- Possibility of double charging: high if duplicate submissions are not prevented or deduplicated by the provider
- Need for manual intervention: yes, often. The reconciliation job will likely require investigation and correction.
- Evidence required: local invoice state, payment history from PayEx, timestamp comparison, reconciliation records, and possibly provider statements or audit logs

This is the weakest option for a last-day-of-month deadline, because it assumes the mismatch will be discovered later instead of being prevented at the source.

The difference between guarantees and assumptions:

- The app can guarantee that it recorded a payment attempt locally if it writes durable state.
- The app cannot guarantee what PayEx did unless the provider offers query, callback, or idempotent semantics.
- So the real guarantee in any of these designs is only as strong as the external payment provider contract.

## Build and run cost

### Option 1 — Idempotency key + deduplication table

- Development effort: Medium
- Database changes: Medium
- New processes/services: Low
- Monitoring: Medium
- Operational support: Medium
- Testing: Medium
- Failure recovery: Medium
- Long-term maintenance: Medium

This is a fairly straightforward design. It is not cheap, but it is comprehensible and manageable. The biggest risk is not the implementation; it is whether PayEx actually supports the required semantics.

### Option 2 — Transactional outbox + worker

- Development effort: High
- Database changes: High
- New processes/services: High
- Monitoring: High
- Operational support: High
- Testing: High
- Failure recovery: Medium
- Long-term maintenance: Medium to High

This is a bigger engineering and operational commitment. It gives better resilience, but it is more expensive to build and support. It needs stronger operational discipline and clearer recovery procedures.

### Option 3 — Optimistic submission + nightly reconciliation

- Development effort: Low
- Database changes: Low to Medium
- New processes/services: Low
- Monitoring: Medium
- Operational support: High
- Testing: Medium
- Failure recovery: High
- Long-term maintenance: High

It is cheap at the beginning, but expensive in operations and risk over time. This approach becomes costly because the team spends time reconciling mismatches instead of preventing them.

## Exactly-once semantics

An outbox table or a deduplication table by itself does not guarantee true exactly-once behavior across the application and PayEx.

There are different levels of correctness:

- Exactly-once processing: the system guarantees that a logical action is performed once and only once across all relevant systems
- At-most-once charging: the system ensures it will not charge more than once, but it may fail to charge when the provider cannot prove whether the request already happened
- Effectively-once business behavior: the app uses a stable business key, persistent records, and reconciliation to make the customer experience look like a single successful payment even if there were retries and recovery steps

This matters because even a durable dedup table or outbox row is not enough if the external provider does not support deduplication or status lookup. If PayEx can accept the same request twice and charge twice, then the application cannot guarantee exact-once by itself. The provider has to cooperate.

So the practical business goal is usually to get to effectively-once behavior with idempotency and reconciliation, not to claim perfect exactly-once processing in a distributed system.

## Revisit criteria

We should revisit this decision if any of the following happen:

- PayEx does not support the required idempotency semantics
- PayEx does not provide reliable status lookup
- the payment flow becomes asynchronous and is not closely tied to a single user action
- payment volume or concurrency increases significantly
- a worker or broker is introduced for other reasons and the architecture is already moving toward async processing
- stronger audit or recovery requirements are introduced by compliance, finance, or regulatory needs

If these conditions are true, the benefit of an outbox + worker pattern becomes more compelling.

## Open questions / prerequisites

The following PayEx questions must be answered before implementation. They are not facts unless the repository contains evidence:

- Does PayEx support idempotency keys?
- How long are those keys retained?
- Can payment status be queried by idempotency key or payment reference?
- What happens when the same idempotency key is submitted again?
- Are callbacks or webhooks available for async completion?
- What are the provider's retry semantics?
- Is there a billing or status API that lets us reconcile payment state after a timeout or crash?

These are required questions because the repository does not contain a confirmed PayEx contract.

## Where the agent was wrong or useless

This section records the main limitations I found while doing the analysis.

The first recommendation for Option 1 was made without evidence that PayEx supports idempotency keys. That was a real weakness in the analysis. A local deduplication table is only useful if the provider also treats repeated keys safely.

The repository does not contain a confirmed PayEx contract, so provider behavior had to remain an explicit unknown. We could compare patterns and failure modes, but we could not determine the actual PayEx API guarantees from this repository alone.

An idempotency key in the local database alone cannot prevent duplicate charging if PayEx does not deduplicate the external request. That is a major point in this decision. The local app can record that the same key was used, but it cannot prevent a second charge at the provider unless the provider honors that key.

The recommendation became more reliable only after I compared it with the outbox option and tested it against a crash-and-retry scenario. The 3 a.m. example makes the dependency on PayEx behavior clear.

The agent could compare architecture patterns, but it could not determine the actual PayEx contract from this repository. That limitation had to stay in the documentation and in the decision. The result is a decision that is conditional on provider capability verification, not a claim that the platform already supports exact-once payment behavior.

## Final recommendation

Choose Option 1 first, but only conditionally.

Before implementation, verify the PayEx contract. I would not claim true exactly-once semantics or assume safe retries until idempotency and status lookup are confirmed.

The team should use a stable payment key, durable local dedup state, and explicit reconciliation logic. If the provider contract does not support the required behavior, or if the workflow grows into a more asynchronous model, the team should revisit this decision and move toward Option 2.

This is the practical and honest answer for a team working from this repository: start simple, verify the important provider contract, and upgrade the design only when the operational or payment requirements force it.
