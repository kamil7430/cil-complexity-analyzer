namespace CilInjecting.Tests.Infrastructure.Fakes;

using Mono.Cecil;
using CilInjection.Core;

public class FakeWeaver : IWeaver
{
    public void Inject(ModuleDefinition module) {}
}