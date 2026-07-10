using System.Reflection;
using System.Runtime.CompilerServices;
using ArchUnitNET.Fluent.Syntax.Elements.Types.Classes;
using ArchUnitNET.xUnit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ECommerce.Application.Abstractions.Messaging;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ECommerce.ArchitectureTests;

public class NamingConventionTests : BaseTest
{
    [Fact]
    public void QueryHandlers_Should_Have_Name_Ending_With_QueryHandler()
    {
        ClassesShouldConjunction rule = Classes()
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))  
            .Should()
            .HaveNameEndingWith("QueryHandler");

        rule.Check(Architecture);
    }

    [Fact]
    public void CommandHandlers_Should_Have_Name_Ending_With_CommandHandler()
    {
        ClassesShouldConjunction rule = Classes()
            .That()
            .ImplementInterface(typeof(ICommandHandler<,>))
            .Should()
            .HaveNameEndingWith("CommandHandler");

        rule.Check(Architecture);
    }

    [Fact]
    public void All_Async_Methods_Should_End_With_Async()
    {
        Assembly[] assemblies =
        [
            typeof(Application.AssemblyMarker).Assembly,
            typeof(Infrastructure.AssemblyMarker).Assembly, 
            typeof(Api.AssemblyMarker).Assembly
        ];

        var violations = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(m => m.DeclaringType?.Assembly == typeof(Application.AssemblyMarker).Assembly ||
                    m.DeclaringType?.Assembly == typeof(Infrastructure.AssemblyMarker).Assembly ||
                    m.DeclaringType?.Assembly == typeof(Api.AssemblyMarker).Assembly)
            .Where(static t => !t.Name.StartsWith("<"))
            .Where(t => !t.FullName!.Contains("+<>"))
            .SelectMany(t => t.GetMethods(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Instance |
                BindingFlags.Static))
            .Where(m => IsAsyncMethod(m))
            .Where(m => !IsPipelineBehavior(m.DeclaringType!))
            .Where(m => m.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
            .Where(m => !typeof(ControllerBase).IsAssignableFrom(m.DeclaringType))
            .Where(m => !IsHandler(m.DeclaringType!))
            .Where(m => !m.Name.EndsWith("Async"))
            .Select(m => $"{m.DeclaringType!.FullName}.{m.Name}")
            .ToList();

        Assert.True(
            violations.Count == 0,
            "The following async methods do not end with 'Async':\n" +
            string.Join("\n", violations));
    }

    private static bool IsAsyncMethod(MethodInfo method)
    {
        Type returnType = method.ReturnType;

        return returnType == typeof(Task)
               || returnType == typeof(ValueTask)
               || returnType.IsGenericType &&
                   (returnType.GetGenericTypeDefinition() == typeof(Task<>)
                    || returnType.GetGenericTypeDefinition() == typeof(ValueTask<>));
    }

    private static bool IsHandler(Type type) => type.GetInterfaces().Any(i =>
                                                         i.IsGenericType &&
                                                         (
                                                             i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                                                             || i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)
                                                             || i.GetGenericTypeDefinition() == typeof(INotificationHandler<>)
                                                         ));

    private static bool IsPipelineBehavior(Type type) => type.GetInterfaces().Any(i =>
                                                                  i.IsGenericType &&
                                                                  i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));
}
