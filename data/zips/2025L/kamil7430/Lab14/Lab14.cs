using System;
using System.Collections.Generic;
using System.Text;

namespace ASD
{
    /// <summary>
    /// Klasa drzewa prefiksowego z możliwością wyszukiwania słów w zadanej odległości edycyjnej
    /// </summary>
    public class Lab14_Trie : System.MarshalByRefObject
    {
        
        // klasy TrieNode NIE WOLNO ZMIENIAĆ!
        private class TrieNode
        {
            public SortedDictionary<char, TrieNode> childs = new SortedDictionary<char, TrieNode>();
            public bool IsWord = false;
            public int WordCount = 0;
        }

        private TrieNode root;

        public Lab14_Trie()
        {
            root = new TrieNode();
        }

        /// <summary>
        /// Zwraca liczbę przechowywanych słów
        /// Ma działać w czasie stałym - O(1)
        /// </summary>
        public int Count { get { return root.WordCount; } }

        /// <summary>
        /// Zwraca liczbę przechowywanych słów o zadanym prefiksie
        /// Ma działać w czasie O(len(startWith))
        /// </summary>
        /// <param name="startWith">Prefiks słów do zliczenia</param>
        /// <returns>Liczba słów o zadanym prefiksie</returns>
        public int CountPrefix(string startWith)
        {
            TrieNode node = root;
            int n = startWith.Length;

            for (int i = 0; i < n; i++)
            {
                if (!node.childs.TryGetValue(startWith[i], out node))
                    return 0;
            }

            return node.WordCount;
        }

        /// <summary>
        /// Dodaje słowo do słownika
        /// Ma działać w czasie O(len(newWord))
        /// </summary>
        /// <param name="newWord">Słowo do dodania</param>
        /// <returns>True jeśli słowo udało się dodać, false jeśli słowo już istniało</returns>
        public bool AddWord(string newWord)
        {
            if (Contains(newWord))
                return false;

            TrieNode node = root;
            int n = newWord.Length;
            root.WordCount++;

            for (int i = 0; i < n; i++)
            {
                if (node.childs.TryGetValue(newWord[i], out var tmp))
                {
                    tmp.WordCount++;
                    node = tmp;
                }
                else
                {
                    node.childs[newWord[i]] = new TrieNode
                    {
                        WordCount = 1
                    };
                    node = node.childs[newWord[i]];
                }
            }
            node.IsWord = true;

            return true;
        }

        /// <summary>
        /// Sprawdza czy podane słowo jest przechowywane w słowniku
        /// Ma działać w czasie O(len(word))
        /// </summary>
        /// <param name="word">Słowo do sprawdzenia</param>
        /// <returns>True jeśli słowo znajduje się w słowniku, wpp. false</returns>
        public bool Contains(string word)
        {
            TrieNode node = root;
            int n = word.Length;

            for (int i = 0; i < n; i++)
            {
                if (!node.childs.TryGetValue(word[i], out node))
                    return false;
            }

            return node.IsWord;
        }

        /// <summary>
        /// Usuwa podane słowo ze słownika
        /// Ma działać w czasie O(len(word))
        /// </summary>
        /// <param name="word">Słowo do usunięcia</param>
        /// <returns>True jeśli udało się słowo usunąć, false jeśli słowa nie było w słowniku</returns>
        public bool Remove(string word)
        {
            if (!Contains(word))
                return false;

            TrieNode node = root;
            int n = word.Length;

            for (int i = 0; i < n; i++)
            {
                node.WordCount--;
                var tmp = node.childs[word[i]];

                if (tmp.WordCount <= 1)
                    node.childs.Remove(word[i]);

                node = tmp;
            }

            if (node != null)
            {
                node.WordCount--;
                node.IsWord = false;
            }

            return true;
        }

        /// <summary>
        /// Zwraca wszystkie słowa o podanym prefiksie. 
        /// Dla pustego prefiksu zwraca wszystkie słowa ze słownika.
        /// Wynik jest w porządku alfabetycznym.
        /// Ma działać w czasie O(liczba węzłów w drzewie)
        /// </summary>
        /// <param name="startWith">Prefiks</param>
        /// <returns>Wyliczenie zawierające wszystkie słowa ze słownika o podanym prefiksie</returns>
        public List<string> AllWords(string startWith = "")
        {
            int n = startWith.Length;
            TrieNode node = root;
            StringBuilder builder = new StringBuilder(startWith);

            for (int i = 0; i < n; i++)
            {
                if (!node.childs.TryGetValue(startWith[i], out node))
                    return new List<string>();
            }

            var list = new List<string>();
            DFS(node);

            return list;

            void DFS(TrieNode localNode)
            {
                if (localNode.IsWord)
                    list.Add(builder.ToString());

                foreach (var pair in localNode.childs)
                {
                    builder.Append(pair.Key);
                    DFS(pair.Value);
                    builder.Remove(builder.Length - 1, 1);
                }
            }
        }

        /// <summary>
        /// Wyszukuje w słowniku wszystkie słowa w podanej odległości edycyjnej od zadanego słowa
        /// Wynik jest w porządku alfabetycznym ze względu na słowa (a nie na odległość).
        /// Ma działać optymalnie - tj. niedozwolone jest wyszukanie wszystkich słów i sprawdzenie ich odległości
        /// Należy przeszukując drzewo odpowiednio odrzucać niektóre z gałęzi.
        /// Złożoność pesymistyczna (gdy wszystkie słowa w słowniku mieszczą się w zadanej odległości)
        /// O(len(word) * (liczba węzłów w drzewie))
        /// </summary>
        /// <param name="word">Słowo</param>
        /// <param name="distance">Odległość edycyjna</param>
        /// <returns>Lista zawierająca pary (słowo, odległość) spełniające warunek odległości edycyjnej</returns>
        public List<(string, int)> Search(string word, int distance = 1)
        {
            int width = 1;
            foreach (var str in AllWords())
                if (str.Length + 1 > width)
                    width = str.Length + 1;
            int height = word.Length + 1;

            var dist = new int[width, height];

            for (int i = 0; i < width; i++)
                dist[i, 0] = i;
            for (int i = 0; i < height; i++)
                dist[0, i] = i;

            var wordList = new List<(string, int)>();
            var builder = new StringBuilder();

            DFS(root, 0);

            return wordList;

            void DFS(TrieNode node, int depth)
            {
                if (depth > 0)
                {
                    int minDistance = int.MaxValue;
                    for (int i = 1; i < height; i++)
                    {
                        var fromUpperDist = dist[depth, i - 1] + 1;
                        var fromLeftDist = dist[depth - 1, i] + 1;
                        var fromDiagonalDist = dist[depth - 1, i - 1] + (word[i - 1] == builder[depth - 1] ? 0 : 1);
                        dist[depth, i] = Math.Min(Math.Min(fromUpperDist, fromLeftDist), fromDiagonalDist);
                        if (minDistance > dist[depth, i])
                            minDistance = dist[depth, i];
                    }

                    if (minDistance > distance)
                        return;

                    if (dist[depth, height - 1] <= distance && node.IsWord)
                        wordList.Add((builder.ToString(), dist[depth, height - 1]));
                }

                foreach (var pair in node.childs)
                {
                    builder.Append(pair.Key);
                    DFS(pair.Value, depth + 1);
                    builder.Remove(builder.Length - 1, 1);
                }
            }
        }

    }
}