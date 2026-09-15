using Mono.Cecil;

namespace CilComplexityAnalyzer.CilInjection;

internal interface IContainerMethods {}

internal record InstructionCounterMethods(
    MethodDefinition GetCounterMethod,
    MethodDefinition ResetCounterMethod
) : IContainerMethods;


internal record InjectedContainerDescriptor(
    FieldDefinition Field,
    IContainerMethods ContainerMethods
);

internal interface ICilContainerInjector
{
    InjectedContainerDescriptor InjectCounterContainer(ModuleDefinition module);
}