# AI Augmentation Log

## Purpose

This document records where I used AI during the workshop, what I checked myself, and which decisions I kept in the final submission.

I used AI for architecture analysis, documentation, technical decisions, and the code-review evaluation. I treated its suggestions as input only; the final decisions and verification were mine.

## 1. Architecture Analysis and Design

- **AI tools:** ChatGPT, GitHub Copilot (Auto mode), Mermaid MCP.
- **Task:** Analyse the existing .NET Invoice Approval application and document its current and proposed architecture.
- **AI contribution:** Assisted in understanding application components, identifying source-code relationships, preparing architecture documentation, and creating Mermaid diagrams.
- **What I checked:** Compared the architecture details with the source files and separated implemented components from proposed production components.
- **My decision:** Kept the claims supported by the source and removed unsupported assumptions.
- **Evidence:** `docs/architecture-evidence.md`, `docs/architecture/`.

## 2. Architecture Testing

- **AI tools:** ChatGPT, GitHub Copilot (Auto mode).
- **Task:** Define a structural architecture rule and implement an automated test.
- **AI contribution:** Assisted in designing a test that prevents the Domain assembly from referencing the API assembly.
- **What I checked:** Reviewed the test and ran it with the .NET test command.
- **My decision:** Kept the test after it passed.
- **Evidence:** `backend/tests/InvoiceApp.Tests/DomainArchitectureTests.cs`, `docs/architecture-test.md`.

## 3. Exact-once PayEx Decision

- **AI tool:** ChatGPT.
- **Task:** Evaluate approaches for reliable PayEx payment submission.
- **AI contribution:** Compared idempotency keys with deduplication, transactional outbox with a worker, and optimistic submission with reconciliation.
- **What I considered:** Failure scenarios, the 3 a.m. last-day-of-month case, implementation cost, and operational complexity.
- **My decision:** Selected idempotency-based submission only if PayEx confirms the required idempotency and status-lookup behavior.
- **Evidence:** `docs/decisions/exact-once-payex-submission.md`.

## 4. AI Autonomy Policy

- **AI tool:** ChatGPT.
- **Task:** Define an autonomy policy for month-end log triage.
- **AI contribution:** Assisted in defining autonomy levels, controls, promotion requirements, and rollback conditions.
- **What I considered:** The six-person team, the two engineers who are new to the codebase, and the risk of granting the agent operational permissions.
- **My decision:** Set the day-one level to Level 1 — Recommend.
- **Evidence:** `docs/autonomy-policy.md`.

## 5. Code Review Evaluation

- **AI tools:** MAI-Code-1.1-Flash, ChatGPT.
- **Task:** Evaluate AI code-review performance using 12 fixed cases.
- **AI contribution:** MAI-Code-1.1-Flash performed the code-review evaluations using the v1 and v2 prompts. ChatGPT assisted in interpreting findings, comparing results, and calculating evaluation metrics.
- **What I checked:** Compared the results with the reviewer-created answer key, checked the misses and false alarms, and reviewed the limits of the experiment.
- **My decision:** Kept both v1 and v2 results. The changed prompt did not show a consistent improvement.
- **Evidence:** `ops/evals/results.csv`, `ops/evals/results-v2.csv`, `ops/evals/review-prompt-v2.md`, `ops/evals/reference-answer-key.md`, `docs/review-evaluation.md`.

## 6. Lethal Trifecta Risk Audit

- **AI tool:** ChatGPT.
- **Task:** Assess the agent configuration against the lethal trifecta.
- **AI contribution:** Assisted in identifying private-data exposure, untrusted content, and external communication risks, and suggesting controls.
- **Human review:** Checked the repository configuration and distinguished invented sample data from potential real production data.
- **Final decision:** Restricted external communication during incident triage and documented conditions requiring engineering-owner review.
- **Evidence:** `docs/risk-audit.md`, `.mcp.json`.

## 7. Engineering Metrics

- **AI tool:** ChatGPT.
- **Task:** Define engineering metrics and identify measurements supported by repository evidence.
- **AI contribution:** Assisted in defining lead time, change failure rate, and review turnaround, and calculating available measurements.
- **Human review:** Checked the supplied CI timing data and Git history. Distinguished invented CI scenario figures from actual repository-history measurements.
- **Final decision:** Documented supported measurements and explicitly identified metrics that could not be calculated from the available evidence.
- **Evidence:** `docs/engineering-metrics-baseline.md`, `ops/pipeline/run-timings.csv`.

## 8. Human Accountability

I treated AI-generated suggestions as recommendations, not as decisions to accept automatically.

The developer reviewed generated documentation, checked source-code evidence, executed the architecture test, evaluated code-review results, and corrected unsupported assumptions.

I reviewed and accepted the final architecture, technical decisions, autonomy policy, evaluation conclusions, and risk controls.

This document is a summary of AI-assisted work and human decisions, not a verbatim transcript of every interaction.