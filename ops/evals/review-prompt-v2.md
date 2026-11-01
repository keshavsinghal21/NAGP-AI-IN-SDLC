# Code Review Evaluation — Prompt v2

Review the following code diff for defects.

Focus only on defects introduced by the changes in this diff. Inspect the relevant surrounding code when necessary to understand the impact.

Check for:
1. Compilation errors, undefined references, and incorrect method or API usage.
2. Input validation, null handling, and whitespace handling.
3. Authentication and authorization problems.
4. Security vulnerabilities, including injection and sensitive information exposure in logs.
5. Concurrency, thread-safety, and data consistency problems.
6. Exception handling, incorrect HTTP status codes, and misleading success responses.
7. Incorrect business logic or unintended changes in application behaviour.
8. API contract and error-response inconsistencies.

For every finding, provide:
- Severity: Critical, High, Medium, or Low.
- File name and line number.
- The specific code causing the problem.
- A realistic scenario explaining what could go wrong.
- Why the problem was introduced by this diff.
- Confidence: High, Medium, or Low.

Important rules:
- Verify findings against the surrounding code before reporting them.
- Do not report pre-existing problems as defects introduced by the diff.
- Do not report style preferences or speculative concerns as confirmed defects.
- If a potential issue depends on an undocumented business rule, clearly identify that uncertainty.
- If you find no defects, explicitly say "No defects found."
- Do not invent findings just to fill the checklist.