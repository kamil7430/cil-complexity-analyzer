using CilInjection.Core.Abstractions;

namespace CilInjection.Core.Contexts;

using System.Collections.Generic;
using Mono.Cecil;

internal class MethodInjectionContext : IMethodInjectionContext
{
    private readonly List<InstructionInjectionContext> _contexts = new();

    public MethodInjectionContext(MethodDefinition method)
    {
        foreach (var inst in method.Body.Instructions)
        {
            if (InstructionInjectionContext.Create(inst) is { } injectionContext)
            {
                _contexts.Add(injectionContext);
            }
        }
    }
    IReadOnlyList<IInstructionInjectionContext> IMethodInjectionContext.Contexts => _contexts;
    public List<InstructionInjectionContext> GetContexts() => _contexts;
}