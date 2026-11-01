# AI in SDLC Assignment

Repository link: https://github.com/keshavsinghal21/NAGP-AI-IN-SDLC

I created this file as a quick guide to submission. The detailed work and evidence are linked below.

## Requirement

### 1. Architecture

- Documented the current architecture and kept it separate from the planned production design.
- Added Mermaid diagrams for the current system, the approval flow, and the planned target state:
  - [As-built container](docs/architecture/as-built-container.mmd)
  - [Approval sequence](docs/architecture/approval-sequence.mmd)
  - [Target state](docs/architecture/target-state-container.mmd)
- Added source citations and a list of inferred or proposed items in [architecture evidence](docs/architecture-evidence.md).
- Added an architecture test to check that the Domain assembly does not reference the API assembly:
  - [Architecture test](backend/tests/InvoiceApp.Tests/DomainArchitectureTests.cs)
  - Run with: `dotnet test backend/InvoiceApp.sln --filter "FullyQualifiedName~DomainArchitectureTests"`

### 2. PayEx exact-once decision

- Compared three options for PayEx submission: idempotency keys with a deduplication table, a transactional outbox with a worker, and optimistic submission with reconciliation.
- Recorded my decision, the trade-offs, the cost, the failure scenario, and the limitations in [the exact-once decision](docs/decisions/exact-once-payex-submission.md).

### 3. AI autonomy policy

- Chose month-end log triage as the workflow.
- Set the day-one level to **L1 — Recommend** because two of the six engineers are new to the codebase.
- Documented the controls and promotion rules in the [autonomy policy](docs/autonomy-policy.md).

### 4. Review evaluation

- Evaluated the ten supplied cases and added two cases:
  - Case 11: whitespace-only approver validation defect.
  - Case 12: clean comment-only change.
- Ran the baseline set three times, then changed only the review prompt and ran it once more.
- Recorded the caught, missed, and false-alarm results in:
  - [Evaluation summary](docs/review-evaluation.md)
  - [Baseline results](ops/evals/results.csv)
  - [Changed-prompt results](ops/evals/results-v2.csv)
  - [Reference answer key](ops/evals/reference-answer-key.md)

### 5. Risk audit and measurement plan

- Reviewed the risks from private data, untrusted content, and external communication in the [risk audit](docs/risk-audit.md).
- Documented the plan for measuring lead time, change failure rate, and review turnaround in the [engineering metrics baseline](docs/engineering-metrics-baseline.md).
- The two CI numbers I captured were:
  - CI run failure rate: **10%**
  - Median successful pipeline duration: **1,888.5 seconds**

### 6. AI usage record

- Recorded how I used AI, what I checked, and which decisions I made in the [AI augmentation log](docs/ai-augmentation-log.md).

## Demo video

Due to time constraint not able to make the demo video. Code Freeze is coming in my project so due to that work load is high these days.