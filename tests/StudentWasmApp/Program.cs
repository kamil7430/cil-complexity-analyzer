using System;
using System.Linq;
using System.Reflection;

namespace StudentWasmApp;

public class Program
{
    public static void Main()
    {
       RunTest("Test 1 (Range + ToList)", () => ExecuteTest1_RangeToList(10000)); 
       RunTest("Test 2 (Where + Select)", () => ExecuteTest2_WhereSelect(10000)); 
       RunTest("Test 3 (Take 50)", () => ExecuteTest3_Take(10000, 50));
       RunTest("Test D (OrderByDescending)", () => ExecuteTest4_OrderBy(10000));
    }

    private static void TryResetCounter()
    {
        try
        {
            var counterType = Type.GetType("<GlobalCounterContainer>");
            if (counterType != null)
            {
                var resetMethod = counterType.GetMethod("ResetInstructionCount", BindingFlags.Public | BindingFlags.Static);
                if (resetMethod != null)
                {
                    resetMethod.Invoke(null, null);
                    Console.WriteLine("[INFO] licznij instrukcji CIL zostal pomyslnie wyzerowany");
                }
                else
                {
                    Console.WriteLine("[TryResetCounter] nie udalo się stworzyć funkcji ResetInstructionCounter");
                }
            }
            else
            {
                Console.WriteLine("[TryResetCounter] Klasa <GlobalCounterContainer> nie została odnaleziona. Upewnij się, że zinstrumentowane pliki DLL zostały nadpisane w folderze managed/.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TryResetCounter] nie udalo sie zresetowac licznika: {ex.Message}");
        }
    }

    private static void TryPrintCounterResult()
    {
        try
        {
            var counterType = Type.GetType("<GlobalCounterContainer>");
            if (counterType != null)
            {
                var getMethod = counterType.GetMethod("GetInstructionCount", BindingFlags.Public | BindingFlags.Static);
                if (getMethod != null)
                {
                    long count = (long)getMethod.Invoke(null, null)!;
                    Console.WriteLine($"[INSTRUCTION_COUNTER_RESULT]={count}");
                }
                else
                {
                    Console.WriteLine("[TryPrintCounterResult] nie udalo się stworzyć funkcji GetInstructionCount");
                }
            }
            else
            {
                Console.WriteLine("[TryPrintCounterResult] Klasa <GlobalCounterContainer> nie została odnaleziona. Upewnij się, że zinstrumentowane pliki DLL zostały nadpisane w folderze managed/.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TryPrintCounterResult] Błąd podczas odczytu licznika: {ex.Message}");
        }
    }

    public static void ExecuteLinqLogic() // daje 697
    {
        var numbers = Enumerable.Range(1, 100);
        var evenNumbers = numbers.Where(n => n % 2 == 0).ToList();
        int sum = evenNumbers.Sum();

        Console.WriteLine($"[LINQ EXEC] Znaleziono {evenNumbers.Count} liczb parzystych. Suma: {sum}");
    }
    
    public static void ExecuteLinqLogic2() // daje 60097 
    {
        var numbers = Enumerable.Range(1, 10000);
        var evenNumbers = numbers.Where(n => n % 2 == 0).ToList();
        int sum = evenNumbers.Sum();

        Console.WriteLine($"[LINQ EXEC] Znaleziono {evenNumbers.Count} liczb parzystych. Suma: {sum}");
    }

    private static void RunTest(string testName, Action testAction)
    {
        Console.WriteLine($"=== {testName} ===");
        TryResetCounter();
        testAction();
        TryPrintCounterResult();
    }
    
    public static void ExecuteTest1_RangeToList(int n)
    {
        var list = Enumerable.Range(1, n).ToList();
        Console.WriteLine($"[TEST 1] Zrobiono liste o rozmiarze {list.Count}");
    }

    public static void ExecuteTest2_WhereSelect(int n)
    {
        var result = Enumerable.Range(1, n)
            .Where(x => x % 2 == 0)
            .Select(x => x * 10)
            .ToList();
        Console.WriteLine($"[TEST 2] Zrobiono liste o rozmiarze {result.Count}");
    }

    public static void ExecuteTest3_Take(int n, int k)
    {
        var result = Enumerable.Range(1, n)
            .Where(x => x % 2 == 0)
            .Take(k)
            .ToList();
        Console.WriteLine($"[TEST 3] Pobrano {result.Count} elementow z {n}");
    }

    public static void ExecuteTest4_OrderBy(int n)
    {
        var result = Enumerable.Range(1, n)
            .Select(x => n - x)
            .OrderBy(x => x)
            .ToList();
        Console.WriteLine($"[TEST 4] Posortowano {result.Count} elementow");
    }
}