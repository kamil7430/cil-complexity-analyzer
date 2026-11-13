using System;
using System.Linq;
using Microsoft.VisualBasic;

namespace StudentWasmApp;

public class Program
{
    private static Action? _reset;
    private static Func<long>? _get;

    public static void Main(string[] args)
    {
        if (!InitCounter()) return;
        //rozgrzewka - usuwa jednorazowy koszt pierwszego uzycia danej kolecji. 
        CollectionsTestRunner.ExecuteListTest(10, () => { });
        CollectionsTestRunner.ExecuteListTest2(10, () => { });
        CollectionsTestRunner.ExecuteHashSetTest(10, () => { });
        CollectionsTestRunner.ExecuteDictionaryFillTest(10, () => { });
        CollectionsTestRunner.ExecuteDictionaryLookupTest(10, () => { });

        string mode = args.Length > 0 ? args[0].ToLower() : "linq";
        if (mode == "collections")
        {
            Console.WriteLine("[STUDENT WASM] Uruchamianie testow Collections");
            RunTest("Test Collections 1 (List.Contains - O(N))", () => CollectionsTestRunner.ExecuteListTest(10000, _reset!));
            RunTest("Test Collections 2 (List[i] petla - O(N))", () => CollectionsTestRunner.ExecuteListTest2(10000, _reset!));
            RunTest("Test Collections 3 (HashSet.Contains - O(1))", () => CollectionsTestRunner.ExecuteHashSetTest(10000, _reset!));
            RunTest("Test Collections 4 (Dictionary.TryGetValue - O(1))", () => CollectionsTestRunner.ExecuteDictionaryLookupTest(10_000, _reset!));
        }
        else if (mode == "scaling")
        {
            Console.WriteLine("[STUDENT WASM] Uruchamianie testow skalowania");
            foreach (var n in new[] { 50, 200, 800, 200 })
            {
                RunScaling("ListContains", n, () => CollectionsTestRunner.ExecuteListTest(n, _reset!));
                RunScaling("ListLoop", n, () => CollectionsTestRunner.ExecuteListTest2(n, _reset!));
                RunScaling("HashSetContains", n, () => CollectionsTestRunner.ExecuteHashSetTest(n, _reset!));
                RunScaling("DictionaryLookup", n, () => CollectionsTestRunner.ExecuteDictionaryLookupTest(n, _reset!));
                RunScaling("DictionaryFill", n, () => CollectionsTestRunner.ExecuteDictionaryFillTest(n, _reset!));
            }
        }
        else
        {
            Console.WriteLine("[STUDENT WASM] Uruchamianie testow Linq");
            RunTest("Test 1 (Range + ToList)", () => ExecuteTest1_RangeToList(10000));
            RunTest("Test 2 (Where + Select)", () => ExecuteTest2_WhereSelect(10000));
            RunTest("Test 3 (Take 50)", () => ExecuteTest3_Take(10000, 50));
            RunTest("Test 4 (OrderByDescending)", () => ExecuteTest4_OrderBy(10000));
        }
    }

    private static bool InitCounter()
    {
        var type = Type.GetType("CilCounter.GlobalCounter");
        if (type == null)
        {
            Console.WriteLine("[COUNTER] Brak CilCounter.GlobalCounter. Podmien System.Private.CoreLib.dll na zinstrumentowany w folderze managed/.");
            return false;
        }

        _reset = (Action)Delegate.CreateDelegate(typeof(Action), type.GetMethod("ResetInstructionCount")!);
        _get = (Func<long>)Delegate.CreateDelegate(typeof(Func<long>), type.GetMethod("GetInstructionCount")!);
        return true;
    }

    private static void RunTest(string testName, Action testAction)
    {
        Console.WriteLine($"=== {testName} ===");
        _reset!();
        testAction();
        long count = _get!();
        Console.WriteLine($"[INSTRUCTION_COUNTER_RESULT]={count}");
    }
    private static void RunScaling(string name, int n, Action testAction)
    {
        _reset!();
        testAction();
        long count = _get!();
        Console.WriteLine($"[SCALING] {name} n={n} count={count}");
    }

    public static void ExecuteTest1_RangeToList(int n)
    {
        var list = Enumerable.Range(1, n).ToList();
        Console.WriteLine($"[TEST 1] Zrobiono liste o rozmiarze {list.Count}");
    }

    public static void ExecuteTest2_WhereSelect(int n)
    {
        var result = Enumerable.Range(1, n).Where(x => x % 2 == 0).Select(x => x * 10).ToList();
        Console.WriteLine($"[TEST 2] Zrobiono liste o rozmiarze {result.Count}");
    }

    public static void ExecuteTest3_Take(int n, int k)
    {
        var result = Enumerable.Range(1, n).Where(x => x % 2 == 0).Take(k).ToList();
        Console.WriteLine($"[TEST 3] Pobrano {result.Count} elementow z {n}");
    }

    public static void ExecuteTest4_OrderBy(int n)
    {
        var result = Enumerable.Range(1, n).Select(x => n - x).OrderBy(x => x).ToList();
        Console.WriteLine($"[TEST 4] Posortowano {result.Count} elementow");
    }
}