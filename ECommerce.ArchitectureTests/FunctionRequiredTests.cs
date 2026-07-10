using System.Reflection;
using System.Windows.Input;
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
        Assembly commandAssembly = typeof(ECommerce.Application.AssemblyMarker).Assembly;

        IEnumerable<Type> commands = commandAssembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                typeof(ICommand<Response>).IsAssignableFrom(t));

        Type[] handlers = commandAssembly.GetTypes();

        foreach (Type command in commands)
        {
            bool exists = handlers.Any(h =>
                h.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericArguments().Contains(command)));

            exists.Should().BeTrue($"{command.Name} must have a handler");
        }
    }

    [Fact]
    public void Every_Query_Should_Have_A_Handler()
    {
        Assembly assembly = typeof(ECommerce.Application.AssemblyMarker).Assembly;

        IEnumerable<Type> queries = assembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                t.GetInterfaces()
                    .Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IQuery<>)));

        Type[] handlers = assembly.GetTypes();

        foreach (Type query in queries)
        {
            bool exists = handlers.Any(h =>
                h.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericArguments().Contains(query)));

            exists.Should().BeTrue($"{query.Name} must have a handler");
        }
    }

    [Fact]
    public void Every_Command_Should_Have_A_Validator()
    {
        Assembly assembly = typeof(ECommerce.Application.AssemblyMarker).Assembly;

        IEnumerable<Type> commands = assembly.GetTypes()
            .Where(t =>
                !t.IsAbstract &&
                typeof(ICommand).IsAssignableFrom(t));

        Type[] validators = assembly.GetTypes();

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
