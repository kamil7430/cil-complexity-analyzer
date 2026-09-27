namespace CilInjection.Core;

using Mono.Cecil;

public interface IWeaver
{
    void Inject(ModuleDefinition module);
}
