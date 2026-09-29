namespace CilInjection.Core.Utils;

using System.Collections.Generic;
using System.Linq;
using Contexts;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

internal class MethodProcessor
{
    private readonly ILInjector _ilInjector = new();
    // ToDo
    // private readonly ILTransformer _ilTransformer = new();

    public void ProcessMethod(MethodDefinition method, IReadOnlyList<IInjectionStrategy> strategies)
    {
        // Uproszczenie makr skoków krótkich (br.s -> br) przed jakimikolwiek zmianami
        method.Body.SimplifyMacros();

        // FAZA 1: Modyfikacje in-place / podmiany / usuwanie instrukcji
        var transformContext = new MethodTransformationContext(method);
        foreach (var strategy in strategies)
        {
            strategy.Transform(transformContext);
        }
        
        // ToDo
        // _ilTransformer.ApplyTransformations(method, transformContext);

        // FAZA 2: Wstrzykiwanie nowego kodu (Before / After) na przetransformowanym IL
        var injectionContext = new MethodInjectionContext(method);
        foreach (var strategy in strategies)
        {
            strategy.Inject(injectionContext);
        }
        
        _ilInjector.ApplyInjections(method, injectionContext);

        // Re-optymalizacja skoków długich do krótkich (br -> br.s) po zakończeniu modyfikacji
        method.Body.OptimizeMacros();
    }
}