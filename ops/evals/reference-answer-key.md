# Evaluation Reference Answer Key

## Purpose

I used this document as the reference when assessing the evaluation cases.

There is no official answer key in the repository. The findings below are my reference list, based on the diffs and the available source context. They are not independently verified ground truth.

## Preliminary case classification

| Case | Expected classification | Expected finding |
|---|---|---|
| 01 | Defect | Nullable request data and undefined references; check role authorization, including Admin access. |
| 02 | Defect | SQL injection risk and undefined dependency/reference. |
| 03 | Ambiguous | Rounding behaviour changed. The intended business rounding rule is not documented. |
| 04 | Clean | Private field rename; no known functional defect. |
| 05 | Defect | Authorization and undefined references; possible incorrect handling of missing invoices. |
| 06 | Clean | Test additions appear consistent with the existing approval service. |
| 07 | Defect | Sensitive invoice and session information may be written to logs; also check undefined references. |
| 08 | Defect | Thread-safety regression and non-atomic dictionary operations. |
| 09 | Defect | README guidance is inconsistent with the application's demo-account and reset behaviour. |
| 10 | Defect | Catch-all exception handling can return HTTP 200 for failed operations. |
| 11 | Defect | `string.IsNullOrEmpty` allows a whitespace-only approver value. |
| 12 | Clean | The change only adds a comment to the authentication middleware. |

## Scoring notes

- Cases 01, 02, 05, 07, 08, 09, 10 and 11 are provisionally classified as defects.
- Cases 04, 06 and 12 are provisionally classified as clean.
- Case 03 is ambiguous and should be reported separately.
- A review that finds an unrelated issue should not automatically count as catching the expected defect.
- Important individual findings, such as sensitive-data logging and authorization gaps, should also be tracked separately.

## Limitations

I prepared this reference by inspecting the repository and the diffs. Some findings may still need more source-code verification, so the results measure agreement with this list rather than accuracy against an official answer key.