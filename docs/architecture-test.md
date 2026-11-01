# Architecture Test for Domain/API Separation

## 1. Rule being checked

The rule I chose is:

- `InvoiceApp.Domain` must not depend on `InvoiceApp.Api`.

In practical terms, the business logic must not reference the web/API project. The Domain project should stay focused on invoice rules, state transitions, and model behavior. HTTP, authentication, and controllers belong in the API project.

## 2. Why this rule matters

I chose this rule because it is simple to check and protects a boundary that matters in this application.

The Domain project represents the invoice workflow and business rules. The API layer takes HTTP requests and translates them into service calls. If the Domain project starts depending on the API project, business logic becomes tied to controllers, middleware, and other HTTP concerns.

That would make the system harder to test and maintain. A business rule should not need to know about the web layer.

## 3. Implementation

The test is implemented in:

- `backend/tests/InvoiceApp.Tests/DomainArchitectureTests.cs`

The class is:

- `DomainArchitectureTests`

The test uses this code:

```csharp
using System.Reflection;
using InvoiceApp.Domain;
using Xunit;

namespace InvoiceApp.Tests;

public class DomainArchitectureTests
{
    [Fact]
    public void DomainAssembly_ShouldNotReferenceApiAssembly()
    {
        var referencedAssemblies = typeof(ApprovalService).Assembly.GetReferencedAssemblies();

        Assert.DoesNotContain(
            referencedAssemblies,
            assembly => assembly.Name == "InvoiceApp.Api");
    }
}
```

The test checks the references in the compiled Domain assembly:

- `typeof(ApprovalService).Assembly` gets the assembly for the `InvoiceApp.Domain` project.
- `GetReferencedAssemblies()` lists all assemblies that the Domain assembly references.
- The assertion checks that `InvoiceApp.Api` is not in that list.

If someone adds a dependency from `InvoiceApp.Domain` to `InvoiceApp.Api`, the test will fail.

## 4. How to run

From the repository root, run:

```bash
dotnet test backend/InvoiceApp.sln --filter "FullyQualifiedName~DomainArchitectureTests"
```

## 5. Test result

The actual terminal evidence available for this session shows the command was run successfully:

- Command: `dotnet test backend/InvoiceApp.sln --filter "FullyQualifiedName~DomainArchitectureTests"`
- Exit code: `0`

So the test passed in the environment where I ran it.

## 6. Limitations

This test checks compiled assembly references only. It is useful for catching a direct dependency from the Domain project to the API project, but it does not catch every possible architectural issue.

For example:

- it does not detect every runtime interaction,
- it does not detect a hidden indirect dependency through another assembly,
- it does not prove that all business logic is perfectly separated in every possible scenario.

It is a useful guardrail, but it is not a complete architecture audit.
