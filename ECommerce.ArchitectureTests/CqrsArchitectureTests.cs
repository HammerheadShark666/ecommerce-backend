using System.Reflection;
using FluentAssertions;
using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.ArchitectureTests;

public class CqrsArchitectureTests
{
    [Fact]
    public void Every_Command_Should_Have_Exactly_One_Handler()
    {
        Assembly assembly = typeof(ECommerce.Application.AssemblyMarker).Assembly;

        var commands = assembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(ICommand<>)))
            .ToList();

        var failures = new List<string>();

        foreach (Type? command in commands)
        {
            int handlerCount = assembly.GetTypes()
                .Count(t =>
                    t.GetInterfaces()
                        .Any(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>) &&
                            i.GenericTypeArguments[0] == command));

            if (handlerCount != 1)
            {
                failures.Add(
                    $"{command.Name} has {handlerCount} handlers");
            }
        }

        failures.Should().BeEmpty(
            string.Join(Environment.NewLine, failures));
    }

    [Fact]
    public void Every_Query_Should_Have_Exactly_One_Handler()
    {
        Assembly assembly = typeof(ECommerce.Application.AssemblyMarker).Assembly;

        var queries = assembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IQuery<>)))
            .ToList();

        var failures = new List<string>();

        foreach (Type? query in queries)
        {
            int handlerCount = assembly.GetTypes()
                .Count(t =>
                    t.GetInterfaces()
                        .Any(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>) &&
                            i.GenericTypeArguments[0] == query));

            if (handlerCount != 1)
            {
                failures.Add(
                    $"{query.Name} has {handlerCount} handlers");
            }
        }

        failures.Should().BeEmpty(
            string.Join(Environment.NewLine, failures));
    }
}
