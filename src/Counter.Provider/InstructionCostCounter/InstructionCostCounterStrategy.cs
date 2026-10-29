using Counter.Provider.Base;

namespace Counter.Provider.InstructionCostCounter;

public class InstructionCostCounterStrategy() : BaseCounterStrategy(new InstructionCostCounterWeaver())
{
}