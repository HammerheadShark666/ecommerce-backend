using System.Reflection;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetArchTest.Rules;

namespace ECommerce.ArchitectureTests;

public class EfConfigurationsArchitectureTests
{
    [Fact]
    public void Ef_Configurations_Should_Only_Exist_In_Infrastructure()
    {
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        var configurationTypes = assemblies
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return [];
                }
            })
            .Where(t =>
                !t.IsAbstract &&
                t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() ==
                    typeof(IEntityTypeConfiguration<>)))
            .ToList();

        var failures = configurationTypes
            .Where(t =>
                t.Assembly != typeof(Infrastructure.AssemblyMarker).Assembly)
            .Select(t =>
                $"{t.FullName} should be in Infrastructure")
            .ToList();

        failures.Should().BeEmpty(
            string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Features_Should_Not_Contain_Ef_Configurations()
    {
        Assembly assembly = typeof(Application.AssemblyMarker).Assembly;

        var failures = assembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() ==
                    typeof(IEntityTypeConfiguration<>)))
            .Select(t => t.FullName)
            .ToList();

        failures.Should().BeEmpty(
            string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Domain_Should_Not_Contain_Ef_Configurations()
    {
        TestResult result = Types.InAssembly(
                typeof(Domain.AssemblyMarker).Assembly)
            .ShouldNot()
            .ImplementInterface(
                typeof(IEntityTypeConfiguration<>))
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
     
    [Fact]
    public void All_Configurations_Should_Be_Located_In_Configurations_Namespace()
    {
        Assembly assembly = typeof(Infrastructure.AssemblyMarker).Assembly;

        var failures = assembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() ==
                    typeof(IEntityTypeConfiguration<>)))
            .Where(t =>
                t.Namespace is null ||
                !t.Namespace.Contains(".Configurations"))
            .Select(t => t.FullName)
            .ToList();

        failures.Should().BeEmpty(
            string.Join(Environment.NewLine, failures));
    }
}
