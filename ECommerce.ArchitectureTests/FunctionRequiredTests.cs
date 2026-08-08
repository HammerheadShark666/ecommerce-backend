using Azure;
using FluentAssertions;
using FluentValidation;
using ECommerce.Application.Abstractions.Messaging;

namespace ECommerce.ArchitectureTests;

public class FunctionRequiredTests
{
    [Fact]
    public void Every_Command_Should_Have_A_Handler()
    {
        var commandAssembly = typeof(ECommerce.Application.AssemblyMarker).Assembly;

        var commands = commandAssembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                typeof(ICommand<Response>).IsAssignableFrom(t));

        Type[] handlers = commandAssembly.GetTypes();

        foreach (Type command in commands)
        {
            var exists = handlers.Any(h =>
                h.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericArguments().Contains(command)));

            exists.Should().BeTrue($"{command.Name} must have a handler");
        }
    }

    [Fact]
    public void Every_Query_Should_Have_A_Handler()
    {
        var assembly = typeof(ECommerce.Application.AssemblyMarker).Assembly;

        var queries = assembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IQuery<>)));

        var handlers = assembly.GetTypes();

        foreach (var query in queries)
        {
            var exists = handlers.Any(h =>
                h.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericArguments().Contains(query)));

            exists.Should().BeTrue($"{query.Name} must have a handler");
        }
    }

    [Fact]
    public void Every_Command_Should_Have_A_Validator()
    {
        var assembly = typeof(ECommerce.Application.AssemblyMarker).Assembly;

        var commands = assembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                typeof(ICommand).IsAssignableFrom(t));

        var validators = assembly.GetTypes();

        foreach (Type command in commands)
        {
            bool exists = validators.Any(v =>
                v.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IValidator<>) &&
                    i.GetGenericArguments()[0] == command));

            exists.Should().BeTrue($"{command.Name} must have a validator");
        }
    }
}
