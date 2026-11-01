# Invoice Approval Application Architecture Evidence

## 1. Purpose

I used this document to record the current architecture of the Invoice Approval application from the source code and `ops/README.md`.

The main goal is to keep these three things separate:
- code that is confirmed by the current implementation,
- code or infrastructure that is planned for production but not yet implemented, and
- items that are inferred from the repository shape rather than directly proven.

## 2. Confirmed by Current Source Code

### Application entry point

- File: `backend/src/InvoiceApp.Api/Program.cs`
- Symbol: `WebApplication.CreateBuilder(args)`, `builder.Services`, `app.UseDefaultFiles()`, `app.UseStaticFiles()`, `app.UseMiddleware<SessionAuthMiddleware>()`, `app.MapControllers()`, `app.Run()`
- Evidence:
  - `Program.cs` is the ASP.NET Core startup file for the API project.
  - It registers MVC, JSON enum serialization, a singleton `SessionStore`, a singleton `IInvoiceRepository`, and a scoped `ApprovalService`.
  - It configures static file hosting and session middleware before mapping controllers.

### API/controller and approval endpoint

- File: `backend/src/InvoiceApp.Api/Controllers/ApprovalsController.cs`
- Symbol: `ApprovalsController`, `Approve`, `Reject`
- Evidence:
  - Class-level route: `[Route("api/[controller]")]`
  - Method-level route for approval: `[HttpPost("{invoiceId}/approve")]`
  - Method-level route for rejection: `[HttpPost("{invoiceId}/reject")]`
  - Full approval route: `POST /api/approvals/{invoiceId}/approve`
  - Class-level authorization: `[RequireRole("Approver", "Admin")]`
  - Method: `public ActionResult<Invoice> Approve(string invoiceId) => Handle(() => _approvals.Approve(invoiceId, HttpContext.GetSession().Username));`
  - Method: `public ActionResult<Invoice> Reject(string invoiceId, [FromBody] RejectRequest request) => Handle(() => _approvals.Reject(invoiceId, HttpContext.GetSession().Username, request.Reason));`

- File: `backend/src/InvoiceApp.Api/Controllers/ApiControllerBase.cs`
- Symbol: `ApiControllerBase.Handle`
- Evidence:
  - `Handle` wraps service calls and maps exceptions to HTTP responses.
  - Success returns `Ok(action())`.
  - `KeyNotFoundException` -> `NotFound`, `InvalidOperationException` -> `Conflict`, `ArgumentException` -> `BadRequest`.

- File: `backend/src/InvoiceApp.Api/Controllers/InvoicesController.cs`
- Symbol: `InvoicesController`, `Submit`
- Evidence:
  - `Submit` is exposed as `POST /api/invoices/{id}/submit`.
  - Method: `public ActionResult<Invoice> Submit(string id) => Handle(() => _approvals.Submit(id));`
  - This is the other route in the invoice approval life cycle.

### Service/application logic

- File: `backend/src/InvoiceApp.Domain/ApprovalService.cs`
- Symbol: `ApprovalService`, `Submit`, `Approve`, `Reject`, `Find`, `EnsurePending`
- Evidence:
  - `ApprovalService` owns the invoice lifecycle transitions.
  - `Submit` changes a draft invoice to `PendingApproval`.
  - `Approve` changes a pending invoice to `Approved` and records the approver and timestamp.
  - `Reject` changes a pending invoice to `Rejected`, records the approver, timestamp, and rejection reason.
  - `ApprovalService` depends on `IInvoiceRepository` via its constructor: `public ApprovalService(IInvoiceRepository repository)`.

- File: `backend/src/InvoiceApp.Api/Program.cs`
- Symbol: `builder.Services.AddScoped<ApprovalService>();`
- Evidence:
  - The service class is registered in the API project and used by controllers.

- Clarification:
  - There is no separate Application project in this repository.
  - The current invoice workflow lives in `backend/src/InvoiceApp.Domain/ApprovalService.cs` and is called by the API layer.

### Domain classes and operations

- File: `backend/src/InvoiceApp.Domain/Invoice.cs`
- Symbol: `Invoice`
- Evidence:
  - `Invoice` contains the core state fields:
    - `Id`
    - `CustomerName`
    - `Amount`
    - `Status`
    - `CreatedAt`
    - `DecidedBy`
    - `DecidedAt`
    - `DecisionNote`

- File: `backend/src/InvoiceApp.Domain/InvoiceStatus.cs`
- Symbol: `InvoiceStatus`
- Evidence:
  - Enum values:
    - `Draft`
    - `PendingApproval`
    - `Approved`
    - `Rejected`

- File: `backend/src/InvoiceApp.Domain/IInvoiceRepository.cs`
- Symbol: `IInvoiceRepository`
- Evidence:
  - Repository contract includes:
    - `GetAll()`
    - `GetById(string id)`
    - `GetByIds(IEnumerable<string> ids)`
    - `Add(Invoice invoice)`
    - `Update(Invoice invoice)`
    - `Remove(string id)`

- File: `backend/src/InvoiceApp.Domain/ApprovalService.cs`
- Symbol: `ApprovalService.Approve`, `ApprovalService.Reject`, `ApprovalService.Submit`
- Evidence:
  - This is the domain logic for invoice progression.
  - `Find` and `EnsurePending` enforce valid state transitions.
  - The state machine is explicit in the code.

### Persistence/store implementation

- File: `backend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs`
- Symbol: `InMemoryInvoiceRepository`
- Evidence:
  - The concrete persistence layer is in-memory and not a database.
  - The file comment states: `/// <summary>In-memory store so the demo app runs with no database. Seeded with a few invoices.</summary>`
  - Internal state: `private readonly List<Invoice> _invoices = new()`
  - Methods include `GetAll`, `GetById`, `GetByIds`, `Add`, `Update`, `Remove`.

- File: `backend/src/InvoiceApp.Api/Program.cs`
- Symbol: `builder.Services.AddSingleton<IInvoiceRepository, InMemoryInvoiceRepository>();`
- Evidence:
  - Dependency injection binds the repository abstraction to the in-memory implementation.

### Authentication/authorization

- File: `backend/src/InvoiceApp.Api/Auth/SessionAuthMiddleware.cs`
- Symbol: `SessionAuthMiddleware`, `InvokeAsync`, `HttpContextExtensions.GetSession`
- Evidence:
  - Middleware enforces bearer-token authentication for `/api` routes except `/api/auth/login`.
  - It reads the `Authorization` header and resolves a `UserSession` from `SessionStore`.
  - If the token is invalid, it returns `401 Unauthorized`.
  - It stores the session in `HttpContext.Items[HttpContextExtensions.SessionKey]`.

- File: `backend/src/InvoiceApp.Api/Auth/SessionStore.cs`
- Symbol: `SessionStore`, `Create`, `Get`, `Remove`
- Evidence:
  - This in-memory session store manages bearer tokens and user sessions.
  - It creates random opaque tokens using `RandomNumberGenerator.GetBytes(16)`.

- File: `backend/src/InvoiceApp.Api/Auth/UserSession.cs`
- Symbol: `UserSession`, `DemoUser`, `DemoUsersOptions`
- Evidence:
  - `UserSession` is a record with `Username` and `Role`.
  - Demo users and roles are configured via `DemoUsersOptions`.

- File: `backend/src/InvoiceApp.Api/Auth/RequireRoleAttribute.cs`
- Symbol: `RequireRoleAttribute`, `OnActionExecuting`
- Evidence:
  - Role enforcement returns `403 Forbidden` if the current session role is not allowed.
  - The approval controller is protected by `[RequireRole("Approver", "Admin")]`.

- File: `backend/src/InvoiceApp.Api/Controllers/AuthController.cs`
- Symbol: `AuthController`, `Login`, `Logout`
- Evidence:
  - Login validates `Username` and `Password` against configured demo users.
  - On success it creates a bearer token and returns `LoginResponse(token, username, role)`.

- File: `backend/src/InvoiceApp.Api/appsettings.json`
- Symbol: `Users`
- Evidence:
  - Demo user accounts and roles are configured in the `Users` section under `appsettings.json`.

### Relevant tests

- File: `backend/tests/InvoiceApp.Tests/ApprovalServiceTests.cs`
- Symbol: `ApprovalServiceTests`, `Submit_MovesDraftToPendingApproval`, `Submit_RejectsInvoiceThatIsNotADraft`, `Approve_SetsStatusApproverAndTimestamp`, `Approve_RejectsInvoiceThatIsStillADraft`, `Approve_ThrowsWhenInvoiceDoesNotExist`, `Reject_RecordsReasonAndApprover`, `Reject_RequiresAReason`
- Evidence:
  - These tests validate the approval lifecycle and state transition rules.
  - They cover submission, approval, rejection, missing invoice, and invalid reason handling.

- File: `backend/tests/InvoiceApp.Tests/FakeInvoiceRepository.cs`
- Symbol: `FakeInvoiceRepository`
- Evidence:
  - The tests use a fake repository implementing `IInvoiceRepository` to validate the domain logic without the API persistence implementation.

### Actual external integrations, if any

- Result: No external integration was found in the inspected source code.
- Evidence:
  - No outbound HTTP client, database client, queue client, or payment-provider client was identified in the inspected application code.
  - `Program.cs` only adds controllers, static files, and in-memory infrastructure.
  - `InMemoryInvoiceRepository` is the current persistence implementation.
  - No `PayEx` or other external service client was found in the repository.
- Conclusion:
  - Based on the inspected source code, there is no confirmed external integration in the as-built application.

## 3. Current Invoice Approval Flow

### HTTP endpoint

- File: `backend/src/InvoiceApp.Api/Controllers/ApprovalsController.cs`
- Symbol: `ApprovalsController.Approve`
- HTTP endpoint: `POST /api/approvals/{invoiceId}/approve`
- Evidence:
  - Class-level route: `[Route("api/[controller]")]`
  - Method-level route: `[HttpPost("{invoiceId}/approve")]`

### Controller/action

- File: `backend/src/InvoiceApp.Api/Controllers/ApprovalsController.cs`
- Symbol: `ApprovalsController.Approve`
- Evidence:
  - `public ActionResult<Invoice> Approve(string invoiceId) => Handle(() => _approvals.Approve(invoiceId, HttpContext.GetSession().Username));`

### Service/application method

- File: `backend/src/InvoiceApp.Domain/ApprovalService.cs`
- Symbol: `ApprovalService.Approve`
- Evidence:
  - `Approve` gets the invoice, checks it is pending, updates status and decision metadata, and persists the result.

### Domain operation

- File: `backend/src/InvoiceApp.Domain/ApprovalService.cs`
- Symbol: `ApprovalService.Find`, `ApprovalService.EnsurePending`, `ApprovalService.Approve`
- Evidence:
  - `Find` retrieves the invoice by id via the repository.
  - `EnsurePending` verifies `invoice.Status == InvoiceStatus.PendingApproval`.
  - `Approve` sets:
    - `invoice.Status = InvoiceStatus.Approved;`
    - `invoice.DecidedBy = approver;`
    - `invoice.DecidedAt = DateTime.UtcNow;`

### Persistence/store operation

- File: `backend/src/InvoiceApp.Domain/IInvoiceRepository.cs`
- Symbol: `IInvoiceRepository.Update`
- File: `backend/src/InvoiceApp.Api/InMemoryInvoiceRepository.cs`
- Symbol: `InMemoryInvoiceRepository.Update`
- Evidence:
  - `ApprovalService.Approve` calls `_repository.Update(invoice)`.
  - `InMemoryInvoiceRepository.Update` replaces the matching invoice in the in-memory list.

### Response returned to caller

- File: `backend/src/InvoiceApp.Api/Controllers/ApiControllerBase.cs`
- Symbol: `ApiControllerBase.Handle`
- Evidence:
  - On success, `Handle` returns `Ok(action())`.
  - Therefore the approval endpoint returns `200 OK` with the updated `Invoice` as the body.
  - Failure mapping:
    - `KeyNotFoundException` -> `NotFound`
    - `InvalidOperationException` -> `Conflict`
    - `ArgumentException` -> `BadRequest`

## 4. Planned but Not Currently Implemented

This section is based on `ops/README.md` and is not a description of the current code.

### Production database

- File: `ops/README.md`
- Evidence:
  - `ops/README.md` states that the planned production version includes a database.
  - The file explicitly says: `None of that is in the code yet`.
- Conclusion:
  - Production database: planned, not present in current source code.

### Message topic/broker

- File: `ops/README.md`
- Evidence:
  - `ops/README.md` describes a planned production scenario with a message topic.
  - The same section says: `None of that is in the code yet`.
- Conclusion:
  - Message topic/broker: planned, not implemented in the current code.

### PayEx integration

- File: `ops/README.md`
- Evidence:
  - `ops/README.md` says the planned production service includes an integration with an external payment provider called PayEx.
  - It also states: `None of that is in the code yet`.
- Conclusion:
  - PayEx integration: planned, not present in current source code.

### Other relevant planned production components

- File: `ops/README.md`
- Evidence:
  - `ops/README.md` mentions an operational model including a `24x7 on-call rotation`.
  - It also references incident packs, pipeline artifacts, and evaluations under `ops/`.
- Conclusion:
  - These are operational planning artifacts and production expectations, not runtime code in the current application.

## 5. Inferred items

The following points are my interpretation of the repository rather than direct code facts.

1. The application is intentionally a demo-only system.
  - Why I inferred it:
     - `README.md` states it is intentionally simple and uses an in-memory store.
     - `InMemoryInvoiceRepository` uses a local `List<Invoice>` and there is no durable persistence layer.
   - This is consistent with the current repository, but it is an inference from the repository shape and documentation.

2. The system is single-process and in-memory for the duration of the app.
  - Why I inferred it: 
     - `Program.cs` registers a singleton repository and a singleton session store.
     - `InMemoryInvoiceRepository` keeps data in memory rather than a durable store.
   - This is consistent with the current code, but it is not a separate runtime architecture statement beyond the repository itself.

3. There is no asynchronous workflow or broker-driven processing in the current implementation.
  - Why I inferred it:
     - The approval logic is synchronous in `ApprovalService` and the controller returns the updated invoice immediately.
     - No queue, worker, or message consumer is present in the current source code.
   - The absence of a broker client is directly observed; the broader architectural implication is inference.

4. The app has no current external payment-processing flow.
  - Why I inferred it:
     - There is no actual payment provider client or call site found in the current source code.
   - This is a repository-limited conclusion and should not be generalized beyond the inspected code.

5. The current app is a thin web/API layer over a small business workflow.
  - Why I inferred it:
     - The domain project contains the lifecycle rules, while the API project exposes endpoints and authentication.
     - The UI is static and acts as a client to the API.
   - This is a reasonable interpretation of the repository structure, but it is not a direct code statement.

## 6. Verification Notes

### Baseline test command

- Command: `dotnet test backend/InvoiceApp.sln`
- Working directory: repository root

### Test result

- Verified from the available terminal context in this session:
  - Last command: `dotnet test backend/InvoiceApp.sln`
  - Exit code: `0`
- Interpretation:
  - The solution test command completed successfully when I ran it.

### Additional verification status

- This document is based on the repository files inspected for the architecture review.
- Any item that could not be directly proven from the repository is marked as planned or inferred, not as a confirmed implementation.
- The `ops/README.md` section was used only to identify planned production components; it was not treated as evidence that those systems exist in the current code.

## Inferred and proposed items

Some elements in the architecture diagrams are conceptual and are not directly implemented as separate components in the current source code.

- **Browser/client:** Represents the user interacting with the application. It is a conceptual participant in the request flow.
- **HTTP request pipeline:** Represents the ASP.NET Core request-processing flow configured in `Program.cs`.
- **Background Worker:** This is a proposed component in the target architecture. There is currently no Worker project, service, or implementation in the source code.
- **Message broker/topic:** This is part of the proposed production design described in `ops/README.md`. It is not currently implemented.
- **Production database:** The current application uses `InMemoryInvoiceRepository`. A production database is a planned component, not an existing integration.
- **PayEx:** PayEx is discussed as a planned external payment integration. There is no active PayEx client or provider implementation in the current source code.

These distinctions are important because the as-built architecture should describe what exists today, while the target-state architecture includes proposed future components.
