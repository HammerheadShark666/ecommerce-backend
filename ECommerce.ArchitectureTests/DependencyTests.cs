using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;

namespace ECommerce.ArchitectureTests;

public class DependencyTests
{
    [Fact]
    public void Application_Should_Not_Depend_On_Anything_Except_Core_Domain()
    {
        TestResult result = Types.InAssembly(typeof(ECommerce.Application.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "ECommerce.Api",
                "ECommerce.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            string.Join(", ", result.FailingTypeNames ?? []));
    }    
    
    [Fact]
    public void Domain_Should_Not_Depend_On_Anything()
    {
        TestResult result = Types.InAssembly(typeof(ECommerce.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "ECommerce.Api",
                "ECommerce.Application",
                "ECommerce.Infrastructure")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_MediatR()
    {
        TestResult result = Types.InAssembly(typeof(ECommerce.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("MediatR")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Domain_Should_Not_Depend_On_EFCore()
    {
        TestResult result = Types.InAssembly(typeof(ECommerce.Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Other_Application()
    {
        Assembly assembly = typeof(ECommerce.Application.AssemblyMarker).Assembly;
  
        var applicationNamespaces = assembly.GetTypes()
            .Where(t => t.Namespace?.Contains(".Features.") == true)
            .Select(t =>
            {
                string ns = t.Namespace!;
                string[] parts = ns.Split('.');

                int idx = Array.IndexOf(parts, "Application");

                return $"{string.Join(".", parts.Take(idx + 1))}.{parts[idx + 1]}";
            })
            .Distinct()
            .ToList();

        foreach (string? application in applicationNamespaces)
        {
            string[] otherApplications = [.. applicationNamespaces.Where(x => x != application)];

            TestResult result = Types.InAssembly(assembly)
                .That()
                .ResideInNamespace(application)
                .ShouldNot()
                .HaveDependencyOnAny(otherApplications)
                .GetResult();

            result.IsSuccessful.Should().BeTrue();
        }
    }
}
