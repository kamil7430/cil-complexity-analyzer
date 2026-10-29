using System;
using System.Collections.Generic;

namespace StudentWasmApp;

/// <summary>
/// UWAGA :
/// COLLECTIONS JEST POWALONE: Collections ma problemy ze soba i potrzebuje tez System.Private.CoreLib.
/// Oznacza, ze jezeli tego nie dodasz to bedziesz miec policzone tylko operacje z Pęli.
/// wtedy dla n = 10000 jest to około 160083
/// 
/// </summary>
public static class CollectionsTestRunner
{
    public static void ExecuteListTest(int n, Action resetCounter)
    {
        var list = new List<int>();
        for (int i = 0; i < n; i++)
        {
            list.Add(i);
        }

        resetCounter();

        list.Add(n);
        bool found = list.Contains(n - 1);
        Console.WriteLine($"[TEST LIST] Znaleziono elemnt: {found}");
    }
    
    public static void ExecuteListTest2(int n, Action resetCounter)
    {
        var list = new List<int>();
        for (int i = 0; i < n; i++)
        {
            list.Add(i);
        }

        resetCounter();

        bool found = false;
        int target = n - 1;

        for (int i = 0; i < n; i++)
        {
            if (list[i] == target)
            {
                found = true;
                break;
            }
        }

        Console.WriteLine($"[TEST LIST] Znaleziono elemnt: {found}");
    }

    public static void ExecuteHashSetTest(int n, Action resetCounter)
    {
        var hashSet = new HashSet<int>();
        for (int i = 0; i < n; i++)
        {
            hashSet.Add(i);
        }

        resetCounter();

        bool found = hashSet.Contains(n - 1);
        Console.WriteLine($"[TEST HASHSET] Znaleziono element: {found}");
    }
}