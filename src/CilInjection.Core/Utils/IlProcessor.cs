using System.Collections.Generic;
using System.Linq;
using CilInjection.Core.Abstractions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using CilInjection.Core.Contexts;

namespace CilInjection.Core.Utils;

internal class IlProcessor
{
    private readonly ILInjector _ilInjector = new();
    // ToDo
    // private readonly ILTransformer _ilTransformer = new();
    

    public void ProcessMethod(ModuleDefinition module, IReadOnlyList<IEngineStrategy> strategies)
    {
        var methods = module.GetTypes()
            .SelectMany(t => t.Methods)
            .Where(m => m.HasBody && m.Body.Instructions.Count > 0)
            .ToList();
        
        var metadataContext = new MetadataContext(module);

        foreach (var method in methods)
        {
            // Uproszczenie makr skoków krótkich (br.s -> br) przed jakimikolwiek zmianami
            method.Body.SimplifyMacros();
            
        
            // FAZA 1: Modyfikacje in-place / podmiany / usuwanie instrukcji
            var transformContext = new MethodTransformationContext(method);
            foreach (var strategy in strategies)
            {
                strategy.Transform(transformContext, metadataContext);
            }
        
            // ToDo
            // _ilTransformer.ApplyTransformations(method, transformContext);

            // FAZA 2: Wstrzykiwanie nowego kodu (Before / After) na przetransformowanym IL
            var injectionContext = new MethodInjectionContext(method);
            foreach (var strategy in strategies)
            {
                strategy.Inject(injectionContext, metadataContext);
            }
        
            _ilInjector.ApplyInjections(method, injectionContext);

            // Re-optymalizacja skoków długich do krótkich (br -> br.s) po zakończeniu modyfikacji
            method.Body.OptimizeMacros();
        }

        foreach (var strategy in strategies)
        {
            strategy.InjectType(metadataContext);
        }
    }
}