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
    public static void ExecuteListTest(int n)
    {
        var list = new List<int>();
        for (int i = 0; i < n; i++)
        {
            list.Add(i);
        }

        bool found = list.Contains(n - 1);
        Console.WriteLine($"[TEST LIST] Znaleziono elemnt: {found}");
    }

    public static void ExecuteHashSetTest(int n)
    {
        var hashSet = new HashSet<int>();
        for (int i = 0; i < n; i++)
        {
            hashSet.Add(i);
        }

        bool found = hashSet.Contains(n - 1);
        Console.WriteLine($"[TEST HASHSET] Znaleziono element: {found}");
    }
}