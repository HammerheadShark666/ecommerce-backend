using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ReflectionAssembly = System.Reflection.Assembly;

namespace ECommerce.ArchitectureTests;

public abstract class BaseTest
{ 
    protected static readonly ReflectionAssembly DomainAssembly = typeof(Domain.AssemblyMarker).Assembly;
    protected static readonly ReflectionAssembly ApplicationAssembly = typeof(Application.AssemblyMarker).Assembly;
    protected static readonly ReflectionAssembly InfrastructureAssembly = typeof(Infrastructure.AssemblyMarker).Assembly;
    protected static readonly ReflectionAssembly ApiAssembly = typeof(Api.AssemblyMarker).Assembly;

    protected static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            DomainAssembly,
            ApplicationAssembly,
            InfrastructureAssembly,
            ApiAssembly)
        .Build();
}
