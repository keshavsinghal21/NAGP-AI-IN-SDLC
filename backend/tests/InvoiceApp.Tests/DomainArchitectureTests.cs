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
