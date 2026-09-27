using System.Reflection;
using System.Runtime.Loader;
using CilInstructionCounter.RunTime;

namespace CilInstructionCounter;

public interface ICounterHandle
{
    long GetCounter();
    void ResetCounter();
}

internal class CounterHandle : ICounterHandle
{
    private readonly Func<long> _getCounter;
    private readonly Action _resetCounter;
    
    public CounterHandle(AssemblyLoadContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        string runtimeAssemblyName = typeof(GlobalCounterContainer).Assembly.GetName().Name!;
        string runtimeTypeName = typeof(GlobalCounterContainer).FullName!;
        
        var runtimeAssembly = context.Assemblies
                                  .FirstOrDefault(a => a.GetName().Name == runtimeAssemblyName)
                              ?? throw new InvalidOperationException($"Biblioteka '{runtimeAssemblyName}' nie została załadowana do kontekstu.");

        var containerType = runtimeAssembly.GetType(runtimeTypeName)
                            ?? throw new InvalidOperationException($"Nie odnaleziono typu '{runtimeTypeName}'.");

        var getMethod = containerType.GetMethod(
                            nameof(GlobalCounterContainer.GetCounter), 
                            BindingFlags.Public | BindingFlags.Static)
                        ?? throw new InvalidOperationException($"Nie odnaleziono metody '{nameof(GlobalCounterContainer.GetCounter)}'.");

        var resetMethod = containerType.GetMethod(
                              nameof(GlobalCounterContainer.ResetCounter), 
                              BindingFlags.Public | BindingFlags.Static)
                          ?? throw new InvalidOperationException($"Nie odnaleziono metody '{nameof(GlobalCounterContainer.ResetCounter)}'.");

        _getCounter = getMethod.CreateDelegate<Func<long>>();
        _resetCounter = resetMethod.CreateDelegate<Action>();
    }
    
    public long GetCounter() => _getCounter();

    public void ResetCounter() => _resetCounter();
}