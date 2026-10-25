using Counter.Provider.Base;

namespace Counter.Provider.InstructionCounter;

public class InstructionCounterStrategy() : BaseCounterStrategy(new InstructionCounterWeaver())
{
}