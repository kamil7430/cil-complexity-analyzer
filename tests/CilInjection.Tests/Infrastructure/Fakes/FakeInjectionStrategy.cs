using System;
using System.IO;
using System.Runtime.Loader;
using CilInjection.Core.Abstractions;
using Mono.Cecil;

namespace CilInjection.Tests
{
    /// <summary>
    /// Testowa (fake) strategia z obsługą Runtime, która implementuje jedynie interfejsy i unika 
    /// dziedziczenia po BaseInjectionStrategyWithRuntime oraz wywoływania RuntimeCloner.
    /// </summary>
    public class FakeStrategyWithRuntime : IInjectionStrategy, IEngineStrategy
    {
        private readonly byte[] _staticRuntimeBytes;
        private readonly IWeaver _weaver;
        private readonly IEngineWeaver _engineWeaver;

        // Konstruktor przyjmujący gotowe bajty oraz weavera
        public FakeStrategyWithRuntime(byte[] staticRuntimeBytes, IWeaver weaver)
        {
            _staticRuntimeBytes = staticRuntimeBytes;
            _weaver = weaver ?? throw new ArgumentNullException(nameof(weaver));
            
            // Zapisujemy referencję do wewnętrznego silnika weavera do późniejszego użycia
            _engineWeaver = _weaver.Engine; 
        }

        // Zwracamy samą siebie, ponieważ klasa implementuje IEngineStrategy bezpośrednio
        IEngineStrategy IInjectionStrategy.Engine => this;

        void IEngineStrategy.Transform(IMethodTransformationContext methodTransformationContext, IMetadataContext metadataContext)
        {
            ArgumentNullException.ThrowIfNull(methodTransformationContext);
            ArgumentNullException.ThrowIfNull(metadataContext);

            // Delegujemy transformację do przekazanego weavera
            _engineWeaver.Transform(methodTransformationContext, metadataContext);
        }

        void IEngineStrategy.Inject(IMethodInjectionContext methodInjectionContext, IMetadataContext metadataContext)
        {
            ArgumentNullException.ThrowIfNull(methodInjectionContext);
            ArgumentNullException.ThrowIfNull(metadataContext);

            // Delegujemy iniekcję do przekazanego weavera
            _engineWeaver.Inject(methodInjectionContext, metadataContext);
        }

        void IEngineStrategy.LoadRuntime(AssemblyLoadContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            // Prosta obsługa Runtime BEZ RuntimeClonera. 
            // Ładujemy oryginalne/przekazane bajty bezpośrednio do kontekstu w pamięci.
            if (_staticRuntimeBytes.Length > 0)
            {
                using var memoryStream = new MemoryStream(_staticRuntimeBytes);
                context.LoadFromStream(memoryStream);
            }
        }
    }
}