using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ECommerce.ArchitectureTests;

public class DependencyGuardTests : BaseTest
{    
    [Fact]
    public void ApplicationLayer_ShouldNotDependOn_EntityFramework()
    {
        IArchRule rule = Types()
            .That()
            .ResideInAssembly(ApplicationAssembly)
            .Should()
            .NotDependOnAny(
                Types().That().ResideInNamespace("Microsoft.EntityFrameworkCore"));

        Architecture.CheckRule(rule);
    }
}
