# Engineering Metrics Baseline and Measurement Plan

## 1. Purpose

I want to establish baseline measures for delivery speed, production stability, and review efficiency before the on-call rotation starts.

The repository includes CI timing records for 20 runs. The operations README says the scenario figures are invented, so I treat these as workshop measurements rather than real production telemetry.

## 2. Available Repository Measurements

The ZIP includes Git history containing six commits.

| Measurement | Source | Result |
|---|---|---:|
| Total commits in supplied repository history | `git rev-list --count HEAD` | 6 |
| Elapsed time between earliest and latest commit | `git log --reverse` and latest commit timestamp | 7 days, 1 minute, 32 seconds |

The earliest commit is dated 5 October 2026 at 10:19:03 UTC. The latest commit is dated 12 October 2026 at 10:20:35 UTC.

These are measurements from the Git history included in the repository. They show repository activity, not actual production delivery performance.

### CI dataset — illustrative measurements

Source: `ops/pipeline/run-timings.csv`

The operations README explicitly states that scenario figures are invented. Therefore, the following are calculated dataset results, not verified real CI telemetry:

| Measurement | Calculation | Result |
|---|---|---:|
| CI run failure rate | 2 failed runs / 20 total runs × 100 | 10% |
| Median successful pipeline duration | Median of summed step durations for 18 successful runs | 1,888.5 seconds (31m 28.5s) |

Failed runs: 6 and 14.

I would not present these illustrative figures as actual production performance.

For the assignment, the two captured CI numbers I am reporting are the 10%
CI run failure rate and the 1,888.5-second median successful pipeline
duration. These are CI indicators only; they are not the production change
failure rate or lead time for changes.

## 3. Required Engineering Metrics

### A. Lead Time for Changes

**Definition:** Time from the first commit associated with a change to its successful production deployment.

**Measurement:** Median elapsed time across deployed changes.

**Required sources:**
- Git commit history.
- Pull request history.
- Production deployment records.

**Current status:** Not measurable from the supplied ZIP because production deployment timestamps are not available.

### B. Change Failure Rate

**Definition:** Percentage of production deployments that result in a failure requiring rollback, hotfix, or another corrective production change.

**Formula:**

`Failed production deployments / Total production deployments × 100`

**Required sources:**
- Production deployment history.
- Rollback and hotfix records.
- Incident records linked to deployments.

**Current status:** Not measurable from the supplied ZIP. The CI run failure rate must not be substituted for this metric.

### C. Review Turnaround

**Definition:** Time from pull request creation to its first substantive human review.

**Measurement:** Median elapsed time across reviewed pull requests.

**Required sources:**
- GitHub pull request creation timestamps.
- GitHub review submission timestamps.

**Current status:** Not measurable from the supplied ZIP because complete PR review timestamps are unavailable.

## 4. Reproducible Data Queries

Use the following GitHub CLI commands when repository access is available:

**Pull request and review data:**

`gh pr list --state all --limit 100 --json number,createdAt,mergedAt,reviews`

**CI run history:**

`gh run list --limit 100 --json databaseId,createdAt,conclusion,event,headBranch`

**Production deployment history:**

`gh api repos/NITIN1670/ai-in-sdlc-assignment/deployments --paginate`

Calculate lead time, change failure rate, and review turnaround from the corresponding timestamps and deployment outcomes. Exclude cancelled or incomplete records where appropriate and document the measurement window.

## 5. Measurement Plan

- Capture all three metrics monthly.
- Use a consistent measurement window and definitions.
- Report median lead time and review turnaround.
- Report change failure rate with its numerator and denominator.
- Link failed deployments to incidents and corrective changes.
- Compare future measurements against the initial baseline.
- Do not treat CI failures as production failures.

## 6. Limitations

The supplied CI data provides two measurable workshop indicators, but the repository explicitly identifies its scenario figures as invented. Actual production baselines require verified GitHub review records and deployment data.

For that reason, this initial baseline is a measurement exercise, not evidence of actual production performance.