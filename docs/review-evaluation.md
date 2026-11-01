# Code Review Evaluation

## 1. Objective and Setup

I used this evaluation to see how consistently the AI agent finds defects without raising false alarms.

The evaluation uses the ten supplied cases plus two cases I added. I used a
reviewer-created reference answer key for scoring.

I added Case 11 as a validation defect: `string.IsNullOrEmpty` allows a
whitespace-only approver value. I added Case 12 as a clean case: it only adds
a comment to the authentication middleware. Both were included in the final
evaluation.

| Setting | Baseline (v1) | Changed prompt (v2) |
|---|---|---|
| Model | MAI-Code-1.1-Flash | MAI-Code-1.1-Flash |
| Prompt | v1 | v2 |
| Cases | 12 | 12 |
| Runs per case | 3 | 1 |
| Total evaluations | 36 | 12 |

Only the prompt was changed. The model, cases, diffs, and context remained the same. The exact v2 prompt is saved in `ops/evals/review-prompt-v2.md`.

## 2. Baseline Results (v1)

Results: `ops/evals/results.csv`

Case 03 is excluded from primary metrics because its rounding behaviour depends on an undocumented business rule.

The baseline therefore has 8 clear defect cases and 3 clear clean cases per
run. Case 03 is reported separately rather than forced into either category.

| Metric | Result |
|---|---:|
| Defect classifications correct | 24/24 (100%) |
| Defect classifications missed | 0/24 (0%) |
| Clean classifications correct | 9/9 (100%) |
| False alarms | 0/9 (0%) |

**Important finding-level misses:**
- Case 01: Admin authorization restriction was missed in 2 of 3 runs.
- Case 07: Sensitive invoice/session logging was missed in 2 of 3 runs.

Correctly classifying a case as defective does not mean every individual defect was identified.

## 3. Changed-Prompt Results (v2)

Results: `ops/evals/results-v2.csv`

Case 03 remains excluded for the same reason.

| Metric | Result |
|---|---:|
| Defect cases detected | 7/8 (87.5%) |
| Defect cases missed | 1/8 (12.5%) |
| Clean cases correctly classified | 2/3 (66.7%) |
| False alarms | 1/3 (33.3%) |

**Important findings:**
- Case 07: Sensitive logging issue detected.
- Case 04: False alarm on a clean private-field rename.
- Case 09: Documentation defect missed.
- Case 01: Admin authorization restriction missed.

## 4. Comparison and Conclusion

The v2 prompt improved detection of the sensitive logging issue in Case 07. However, it introduced a false alarm in Case 04 and missed the documentation defect in Case 09. It also missed the Admin authorization restriction in Case 01.

Therefore, v2 did not demonstrate a consistent overall improvement over v1.

## 5. Limitations and Next Steps

- Baseline used three runs per case, while v2 used one. This limits direct comparison.
- Only 12 cases were evaluated, so results cannot be generalized to all code reviews.
- Case 03 was excluded because its intended rounding rule is undocumented.
- The reference answer key was reviewer-created and not independently validated.
- Results measure agreement with the reference key, not absolute accuracy.

**Confidence: Low.** The sample is too small for a strong conclusion. I would need more runs, more cases, and an independent review of the reference answer key.

Next steps:
- Investigate the missed findings and false alarm.
- Run additional evaluations with the same fixed cases.
- Add more security, authorization, concurrency, and clean-change examples.
- Keep human review mandatory for code changes.

Based on these results, I would not allow the agent to approve code changes on its own.