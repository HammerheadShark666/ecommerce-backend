using ArchUnitNET.xUnit;
using ECommerce.Application.Abstractions.Messaging;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ECommerce.ArchitectureTests;

public class VisibilityTests : BaseTest
{
    [Fact]
    public void CommandHandlers_ShouldBeInternal() => Classes()
        .That()
        .ImplementInterface(typeof(ICommandHandler<,>))
        .Should()
        .BeInternal()
        .Check(Architecture);

    [Fact]
    public void QueryHandlers_ShouldBeInternal() => Classes()
            .That()
            .ImplementInterface(typeof(IQueryHandler<,>))
            .Should()
            .BeInternal()
            .Check(Architecture);
}
