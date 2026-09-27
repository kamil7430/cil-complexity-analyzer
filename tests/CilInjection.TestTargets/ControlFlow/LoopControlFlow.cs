namespace TestTargets.ControlFlow;

public class LoopControlFlow
{
    public int ForLoop(int count)
    {
        int total = 0;
        for (int i = 0; i < count; i++)
        {
            total += i;
        }
        return total;
    }

    public int WhileLoopWithBreakAndContinue(int[] numbers)
    {
        int sum = 0;
        int i = 0;
        while (i < numbers.Length)
        {
            int num = numbers[i];
            i++;

            if (num < 0)
            {
                continue; // Skok w przód do ewaluacji warunku pętli / inkrementacji
            }

            if (num == 999)
            {
                break; // Skok w przód poza pętlę
            }

            sum += num;
        }
        return sum;
    }

    public int DoWhileLoop(int initial)
    {
        int val = initial;
        do
        {
            val /= 2;
        } while (val > 10);

        return val;
    }
}