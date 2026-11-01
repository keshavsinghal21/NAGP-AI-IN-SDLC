# AI Autonomy Policy — Month-End Log Triage

## 1. Workflow and scope

This policy covers using an AI agent to help engineers with month-end incident log triage. The agent may inspect incident packs, logs, deployment diffs, CI results, and evaluation evidence. It can summarize an incident, identify error patterns, suggest likely causes, build a timeline, and recommend next steps.

This policy applies to the repository-based triage workflow, not direct production monitoring or operations.

## 2. Autonomy Levels

| Level | Permission |
|---|---|
| **L0 — Observe** | Read approved evidence and summarize facts. |
| **L1 — Recommend** | Analyze evidence, identify likely causes, and suggest actions. Human approval is required for all actions. |
| **L2 — Controlled Assistance** | Create new reports or files in approved locations. No overwriting or deleting original evidence. |
| **L3 — Autonomous Operations** | Execute approved operational actions without case-by-case approval, subject to strict safeguards. |

## 3. Day-One Autonomy Decision

**Day-one level: L1 — Recommend.**

There are six engineers on the team, and two are new to this codebase. The agent has not shown enough reliable performance to make operational decisions on its own. It can analyze and recommend, but an engineer remains responsible for every decision and action.

## 4. Controls and Restrictions

- Use read-only access to source code, incident evidence, logs, and CI results.
- Every finding must reference its supporting file or evidence.
- Clearly distinguish verified facts, hypotheses, and missing or contradictory evidence.
- Do not expose secrets, credentials, tokens, or unnecessary customer information.
- Never independently deploy, rollback, restart services, change configuration, modify permissions, or execute production commands.
- Never initiate payments, approve invoices, close incidents, or send customer communications.
- Human approval is mandatory before any operational or customer-impacting action.

At L2, the agent may create new reports only in an approved output directory. Original evidence must remain unchanged.

## 5. Promotion Evidence and Approval

Promotion from L1 to L2 requires:

- Three consecutive successful runs of the complete evaluation suite.
- Human verification of all evaluation findings.
- No critical missed defect or unauthorized action.
- No modification or deletion of original evidence.
- Documented review and explicit approval.

**Approver:** The engineering owner or the designated on-call lead. The specific person still needs to be assigned when this policy is adopted.

L3 requires a separate risk assessment, stronger safeguards, and explicit approval; passing evaluations alone is insufficient.

## 6. Rollback and Review

Immediately return the agent to L1 or disable its write permissions if it produces an unauthorized change, exposes sensitive information, repeatedly misses critical findings, or acts on unsupported assumptions.

Review this policy whenever the agent's permissions, workflow, model, or evaluation results materially change.

**Principle:** The agent can speed up investigation, but engineers remain accountable for decisions and production actions.