namespace CilInjection.Core;

using Mono.Cecil;

public class FakeWeaver : IWeaver
{
    public void Inject(ModuleDefinition module) {}
}