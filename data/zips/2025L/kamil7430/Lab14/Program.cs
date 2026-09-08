using System;
using System.Collections.Generic;
using System.Linq;
using static ASD.TestCase;

namespace ASD
{
    interface ITrieOperation
    {
        string Name { get; set; }

        void PerformOp(Lab14_Trie trie);
        (Result resultCode, string message) VerifyTestCase();
    }

    class AddTrieOp : ITrieOperation
    {
        private readonly string[] _words;
        private readonly bool[] _expected;
        private List<bool> _result;

        public AddTrieOp(string name, string[] words, bool[] expected)
        {
            _words = words;
            _expected = expected;
            _result = new List<bool>();
            Name = name;
        }

        public string Name { get; set; }

        public void PerformOp(Lab14_Trie trie)
        {
            foreach (var w in _words)
            {
                _result.Add(trie.AddWord(w));
            }
        }

        public (Result resultCode, string message) VerifyTestCase()
        {
            if (_expected == null) return (Result.Success, "Checking skipped");
            if (_result.Count != _expected.Length)
                return (Result.WrongResult, "Diff length");

            for (int i = 0; i < _result.Count; i++)
                if (_result[i] != _expected[i])
                    return (Result.WrongResult, $"FAIL adding word {_words[i]}, expected {_expected[i]}");

            return (Result.Success, "PASS");
        }
    }

    class CountWordTrieOp : ITrieOperation
    {
        private readonly string _prefix;
        private readonly int _expected;
        private int _result;
        public string Name { get; set; }

        public CountWordTrieOp(string name, string prefix, int expected)
        {
            Name = name;
            _prefix = prefix;
            _expected = expected;
        }

        public void PerformOp(Lab14_Trie trie)
        {
            _result = trie.CountPrefix(_prefix);
        }

        public (Result resultCode, string message) VerifyTestCase()
        {
            if (_result == _expected)
                return (Result.Success, "OK");
            return (Result.WrongResult, $"FAIL, expected {_expected} not {_result}");
        }
    }

    class CheckWordsTrieOp : ITrieOperation
    {
        private readonly string[] _words;
        private readonly bool[] _expected;
        private List<bool> _result;
        public string Name { get; set; }

        public CheckWordsTrieOp(string name, string[] words, bool[] expected)
        {
            Name = name;
            _words = words;
            _expected = expected;
            _result = new List<bool>();
        }

        public void PerformOp(Lab14_Trie trie)
        {
            foreach (var w in _words)
                _result.Add(trie.Contains(w));
        }

        public (Result resultCode, string message) VerifyTestCase()
        {
            for (int i = 0; i < _words.Length; i++)
            {
                if (_result[i] != _expected[i])
                    return (Result.WrongResult, $"[FAILED] Wrong answer for word '{_words[i]}' - {_result[i]} ");
            }
            return (Result.Success, "OK");
        }
    }

    class AllWordsTrieOp : ITrieOperation
    {
        private string _prefix;
        private string[] _expectedWords;
        private List<string> _result;
        public string Name { get; set; }

        public AllWordsTrieOp(string name, string prefix, string[] expectedWords)
        {
            Name = name;
            _prefix = prefix;
            _expectedWords = expectedWords;
            Array.Sort(_expectedWords);
        }

        public void PerformOp(Lab14_Trie trie)
        {
            _result = trie.AllWords(_prefix)?.ToList();
        }

        public (Result resultCode, string message) VerifyTestCase()
        {
            if (_expectedWords.Length != _result.Count)
                return (Result.WrongResult, "Diff length");

            for (int i = 0; i < _expectedWords.Length; i++)
            {
                if (_result[i] != _expectedWords[i])
                    return (Result.WrongResult, $"[FAILED] AllWords - Wrong answer for word '{_expectedWords[i]}' - {_result[i]} ");
            }
            return (Result.Success, $"[PASSED] OK");
        }
    }

    class RemoveWordsTrieOp : ITrieOperation
    {
        private readonly string[] _words;
        private readonly bool[] _expected;
        private List<bool> _result;
        public string Name { get; set; }

        public RemoveWordsTrieOp(string name, string[] words, bool[] expected)
        {
            Name = name;
            _words = words;
            _expected = expected;
            _result = new List<bool>();
            if (_words.Length != _expected.Length)
                throw new ArgumentException("Arrays should be the same length!");
        }

        public void PerformOp(Lab14_Trie trie)
        {
            foreach (string w in _words)
            {
                _result.Add(trie.Remove(w));
            }
        }

        public (Result resultCode, string message) VerifyTestCase()
        {
            for (int i = 0; i < _words.Length; i++)
            {
                if (_result[i] != _expected[i])
                    return (Result.WrongResult, $"[FAILED] RemoveWords -  While removing '{_words[i]}' expected {_expected[i]}");
            }
            return (Result.Success, $"[PASSED] OK");
        }
    }

    class SearchTrieOp : ITrieOperation
    {
        private readonly string _word;
        private readonly int _distance;
        private readonly (string, int)[] _expectedWords;
        private List<(string, int)> _result;
        public string Name { get; set; }

        public SearchTrieOp(string name, string word, int distance, (string, int)[] expectedWords)
        {
            Name = name;
            _word = word;
            _distance = distance;
            _expectedWords = expectedWords;
            Array.Sort(_expectedWords, (a, b) => { return a.Item1.CompareTo(b.Item1); });
        }

        public void PerformOp(Lab14_Trie trie)
        {
            var ts = trie.Search(_word, _distance);
            _result = ts!=null ? new List<(string, int)>(ts) : null ;
        }

        public (Result resultCode, string message) VerifyTestCase()
        {
            if ( _result == null)
            {
                return (Result.WrongResult, $"[FAILED] Search - null returned");
            }

            if (_expectedWords.Length != _result.Count)
            {
                return (Result.WrongResult, $"[FAILED] Search - Wrong number of words, should be '{_expectedWords.Length}' but is {_result.Count} ");
            }

            for (int i = 0; i < _expectedWords.Length; i++) 
            {
                if (_result[i].Item1 != _expectedWords[i].Item1 && _result[i].Item2 != _expectedWords[i].Item2)
                    return (Result.WrongResult, $"[FAILED] Search - Wrong answer for word '{_expectedWords[i]}' - {_result[i]} ");
            }
            return (Result.Success, $"[PASSED] OK");
        }
    }
    
    class TrieTestCase : TestCase
    {
        private List<ITrieOperation> operations_;

        public TrieTestCase(double timeLimit, Exception expectedException, string description,
            List<ITrieOperation> operations)
            : base(timeLimit, expectedException, description)
        {
            operations_ = operations;
        }

        protected override void PerformTestCase(object prototypeObject)
        {
            Lab14_Trie trie = new Lab14_Trie();
            foreach (ITrieOperation op in operations_)
            {
                op.PerformOp(trie);
            }
        }

        protected override (Result resultCode, string message) VerifyTestCase(object settings)
        {
            foreach (ITrieOperation op in operations_)
            {
                (Result resultCode, string message) = op.VerifyTestCase();
                if (resultCode != Result.Success)
                {
                    return (resultCode, $"[FAILED] Operation '{op.Name}' failed - {message}");
                }
            }
            return (Result.Success, $"[PASSED] OK, {Description} [{PerformanceTime:#0.00}]");
        }
    }

    class TrieTester : TestModule
    {
        private const int TIME_MULTIPLIER = 1;

        public override void PrepareTestSets()
        {
            TestSets["AddWordOperations"] = AddWordTests();
            TestSets["CountContainsOperations"] = CountContainsTests();
            TestSets["AllWordsOperations"] = AllWordsTests();
            TestSets["BasicTrieOperations"] = RemoveTests();
            TestSets["SearchTrie"] = SearchTrieTests();
        }

        private TestSet AddWordTests()
        {
            var addWordTrieTestSet = new TestSet(new Lab14_Trie(), "Add Word (1 pkt.)");
            
            addWordTrieTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test1 - adding words",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "asdf", "abcd", "abc", "abcde" },
                        new bool[] { true, true, true, true }
                    )
                }));

            addWordTrieTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test2 - adding words",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "asdf", "asdf", "abcd", "abc", "abcde" },
                        new bool[] { true, false, true, true, true }
                    )
                }));

            addWordTrieTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test3 - adding words",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "abc", "xyz", "a", "w", "qwert", "abc", "abcd", "asdf", "qwerty", "a" },
                        new bool[] { true, true, true, true, true, false, true, true, true, false }
                    )
                }));

            addWordTrieTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test4 - adding words",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "abcdef", "abcde", "ab", "abcdef", "abc", "a", "ab", "abcd" },
                        new bool[] { true, true, true, false, true, true, false, true }
                    )
                }));

            return addWordTrieTestSet;
        }

        private TestSet CountContainsTests()
        {
            var countContainsTrieTestSet = new TestSet(new Lab14_Trie(), "CountPrefix i Contains (0.5 pkt.)");
            
            countContainsTrieTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test1 - counting added words",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "asdf", "asdf", "abcd", "abc", "abcde" },
                        new bool[] { true, false, true, true, true }
                    ),
                    new CountWordTrieOp("Count all", "", 4),
                    new CountWordTrieOp("Count a*", "a", 4),
                    new CountWordTrieOp("Count ab*", "ab", 3),
                    new CountWordTrieOp("Count z*", "z", 0),
                }));

            countContainsTrieTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test2 - counting added words",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "abc", "xyz", "a", "w", "qwert", "abcd", "asdf", "qwerty" },
                        new bool[] { true, true, true, true, true, true, true, true }
                    ),
                    new CountWordTrieOp("Count all", "", 8),
                    new CountWordTrieOp("Count a*", "a", 4),
                    new CountWordTrieOp("Count z*", "z", 0),
                    new CountWordTrieOp("Count x*", "x", 1),
                    new CountWordTrieOp("Count xx*", "xx", 0),
                    new CountWordTrieOp("Count q*", "q", 2)
                }));

            countContainsTrieTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test3 - contains check",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "asdf", "asdf", "abcd", "abc", "abcde" },
                        new bool[] { true, false, true, true, true }
                    ),
                    new CheckWordsTrieOp("Word checking",
                        new string[] { "asdf", "xxx", "ab", "abcd", "abcdef", "abc", "abcde" },
                        new bool[] { true, false, false, true, false, true, true }
                    ),
                }));

            countContainsTrieTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test4 - contains check",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "abc", "xyz", "a", "w", "qwert", "abcd", "asdf", "qwerty" },
                        new bool[] { true, true, true, true, true, true, true, true }
                    ),
                    new CheckWordsTrieOp("Word checking",
                        new string[] { "asdf", "xxx", "ab", "abcd", "abcdef", "abc", "abcde", "w", "ww" },
                        new bool[] { true, false, false, true, false, true, false, true, false }
                    ),
                }));

            return countContainsTrieTestSet;
        }

        private TestSet AllWordsTests()
        {
            var allWordsTestSet = new TestSet(new Lab14_Trie(), "AllWords (0.5 pkt.)");
            
            allWordsTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test4 - list all words",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "asdf", "asdf", "abcd", "abc", "abcde" },
                        new bool[] { true, false, true, true, true }
                    ),
                    new AllWordsTrieOp("List all words", "",
                        new string[] { "abc", "abcd", "abcde", "asdf" }
                    )
                }));

            allWordsTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test4 - list all words",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "abc", "xyz", "a", "w", "qwert", "abcd", "asdf", "qwerty" },
                        new bool[] { true, true, true, true, true, true, true, true }
                    ),
                    new AllWordsTrieOp("List all words", "",
                        new string[] { "abc", "xyz", "a", "w", "qwert", "abcd", "asdf", "qwerty" }
                    )
                }));

            allWordsTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test4 - list all words",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "asdf", "xxx", "ab", "abcd", "abcdef", "abc", "abcde", "w", "ww" },
                        new bool[] { true, true, true, true, true, true, true, true, true }
                    ),
                    new AllWordsTrieOp("List all words", "",
                        new string[] { "asdf", "xxx", "ab", "abcd", "abcdef", "abc", "abcde", "w", "ww" }
                    )
                }));
            
            allWordsTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test4 - list all words",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "abcdef", "abcde", "ab", "abc", "a", "abcd" },
                        new bool[] { true, true, true, true, true, true }
                    ),
                    new AllWordsTrieOp("List all words", "",
                        new string[] { "a", "ab", "abc", "abcd", "abcde", "abcdef" }
                    ),
                    new AllWordsTrieOp("List all words", "z",
                        new string[] { }
                    )
                }));
            
            allWordsTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test4 - list all words",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "asdf", "xxx", "ab", "abcd", "abcdef", "abc", "abcde", "w", "ww" },
                        new bool[] { true, true, true, true, true, true, true, true, true }
                    ),
                    new AllWordsTrieOp("List all words start with 'a'", "a",
                        new string[] { "asdf", "ab", "abcd", "abcdef", "abc", "abcde" }
                    ),
                    new AllWordsTrieOp("List all words start with 'w'", "w",
                        new string[] { "w", "ww" }
                    ),
                    new AllWordsTrieOp("List all words start with 'xxx'", "xxx",
                        new string[] { "xxx" }
                    )
                }));

            return allWordsTestSet;
        }

        private TestSet RemoveTests()
        {
            var removeTestSet = new TestSet(new Lab14_Trie(), "Removing (1.0 pkt.)");
            
            removeTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test1 - removing words and count",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "asdf", "asdf", "abcd", "abc", "abcde" },
                        new bool[] { true, false, true, true, true }
                    ),
                    new RemoveWordsTrieOp("Remove words",
                        new string[] { "xxx", "abcd", "abcd", "abcdefg", "abc", "ab" },
                        new bool[] { false, true, false, false, true, false }
                    ),
                    new CountWordTrieOp("Count all", "", 2),
                }));
            
            removeTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test2 - adding removed words and count",
                new List<ITrieOperation> {
                    new AddTrieOp("Adding words",
                        new string[] { "asdf", "asdf", "abcd", "abc", "abcde" },
                        new bool[] { true, false, true, true, true }
                    ),
                    new RemoveWordsTrieOp("Remove words",
                        new string[] { "xxx", "abcd", "abcd", "abcdefg", "abc", "ab" },
                        new bool[] { false, true, false, false, true, false }
                    ),
                    new AddTrieOp("Adding words",
                        new string[] { "asdf", "abcd", "abc", "abcde", "ab", "abcd" },
                        new bool[] { false, true, true, false, true, false }
                    ),
                    new CountWordTrieOp("Count all", "", 5),
                }));

            return removeTestSet;
        }

        private TestSet SearchTrieTests()
        {
            var searchTrieTestSet = new TestSet(new Lab14_Trie(), "Search Trie (1 pkt.)");

            //Simple test for search
            searchTrieTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Simple test for search",
                new List<ITrieOperation> {
                    new AddTrieOp("Add",
                        new string[] { "asdf", "asdf", "abcd", "abc", "abcde" },
                        new bool[] { true, false, true, true, true }
                    ),
                    new SearchTrieOp("Search for 'abc' with dist=2", "abc", 2,
                        new (string, int)[] { ( "abc", 0 ), ( "abcd", 1 ), ( "abcde", 2 ) }
                    )
                }));

            //Simple test for search wint removing
            searchTrieTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Simple test for search with removing",
                new List<ITrieOperation> {
                    new AddTrieOp("Add",
                        new string[] { "book", "books", "cake", "boo", "boon", "cook", "cape", "cart" },
                        new bool[] { true, true, true, true, true, true, true, true }
                    ),
                    new SearchTrieOp("Search for 'caqe' with dist=1", "caqe", 1,
                        new (string, int)[]  { ( "cake", 1 ), ( "cape", 1 )}
                    ),
                    new RemoveWordsTrieOp("Remove", new string[] { "caqe", "cake" }, new bool[] { false, true }),
                    new SearchTrieOp("Search for 'caqe' with dist=1", "caqe", 1,
                        new (string, int)[] { ( "cape", 1 ) }
                    ),
                    new AddTrieOp("Add",
                        new string[] { "cakes", "make", "babe", "cafe", "bike" }, new bool[] { true, true, true, true, true }
                    ),
                    new SearchTrieOp("Search for 'cake' with dist=2", "cake", 2,
                        new (string, int)[] { ( "babe", 2 ), ( "bike", 2), ( "cafe", 1 ), ( "cakes", 1 ), ( "cape", 1), ( "cart", 2 ), ( "make", 1) }.ToArray()
                    ),
                }));

            //Test 1k words search
            searchTrieTestSet.TestCases.Add(new TrieTestCase(TIME_MULTIPLIER, null, "Test 1k words search",
                new List<ITrieOperation> {
                    new AddTrieOp("Add",
                        new string[] { "the", "of", "to", "and", "a", "in", "is", "it", "you", "that", "he", "was", "for", "on", "are", "with", "as", "I", "his", "they", "be", "at", "one", "have", "this", "from", "or", "had", "by", "hot", "word", "but", "what", "some", "we", "can", "out", "other", "were", "all", "there", "when", "up", "use", "your", "how", "said", "an", "each", "she", "which", "do", "their", "time", "if", "will", "way", "about", "many", "then", "them", "write", "would", "like", "so", "these", "her", "long", "make", "thing", "see", "him", "two", "has", "look", "more", "day", "could", "go", "come", "did", "number", "sound", "no", "most", "people", "my", "over", "know", "water", "than", "call", "first", "who", "may", "down", "side", "been", "now", "find", "any", "new", "work", "part", "take", "get", "place", "made", "live", "where", "after", "back", "little", "only", "round", "man", "year", "came", "show", "every", "good", "me", "give", "our", "under", "name", "very", "through", "just", "form", "sentence", "great", "think", "say", "help", "low", "line", "differ", "turn", "cause", "much", "mean", "before", "move", "right", "boy", "old", "too", "same", "tell", "does", "set", "three", "want", "air", "well", "also", "play", "small", "end", "put", "home", "read", "hand", "port", "large", "spell", "add", "even", "land", "here", "must", "big", "high", "such", "follow", "act", "why", "ask", "men", "change", "went", "light", "kind", "off", "need", "house", "picture", "try", "us", "again", "animal", "point", "mother", "world", "near", "build", "self", "earth", "father", "head", "stand", "own", "page", "should", "country", "found", "answer", "school", "grow", "study", "still", "learn", "plant", "cover", "food", "sun", "four", "between", "state", "keep", "eye", "never", "last", "let", "thought", "city", "tree", "cross", "farm", "hard", "start", "might", "story", "saw", "far", "sea", "draw", "left", "late", "run", "while", "press", "close", "night", "real", "life", "few", "north", "open", "seem", "together", "next", "white", "children", "begin", "got", "walk", "example", "ease", "paper", "group", "always", "music", "those", "both", "mark", "often", "letter", "until", "mile", "river", "car", "feet", "care", "second", "book", "carry", "took", "science", "eat", "room", "friend", "began", "idea", "fish", "mountain", "stop", "once", "base", "hear", "horse", "cut", "sure", "watch", "color", "face", "wood", "main", "enough", "plain", "girl", "usual", "young", "ready", "above", "ever", "red", "list", "though", "feel", "talk", "bird", "soon", "body", "dog", "family", "direct", "pose", "leave", "song", "measure", "door", "product", "black", "short", "numeral", "class", "wind", "question", "happen", "complete", "ship", "area", "half", "rock", "order", "fire", "south", "problem", "piece", "told", "knew", "pass", "since", "top", "whole", "king", "space", "heard", "best", "hour", "better", "true", "during", "hundred", "five", "remember", "step", "early", "hold", "west", "ground", "interest", "reach", "fast", "verb", "sing", "listen", "six", "table", "travel", "less", "morning", "ten", "simple", "several", "vowel", "toward", "war", "lay", "against", "pattern", "slow", "center", "love", "person", "money", "serve", "appear", "road", "map", "rain", "rule", "govern", "pull", "cold", "notice", "voice", "unit", "power", "town", "fine", "certain", "fly", "fall", "lead", "cry", "dark", "machine", "note", "wait", "plan", "figure", "star", "box", "noun", "field", "rest", "correct", "able", "pound", "done", "beauty", "drive", "stood", "contain", "front", "teach", "week", "final", "gave", "green", "oh", "quick", "develop", "ocean", "warm", "free", "minute", "strong", "special", "mind", "behind", "clear", "tail", "produce", "fact", "street", "inch", "multiply", "nothing", "course", "stay", "wheel", "full", "force", "blue", "object", "decide", "surface", "deep", "moon", "island", "foot", "system", "busy", "test", "record", "boat", "common", "gold", "possible", "plane", "stead", "dry", "wonder", "laugh", "thousand", "ago", "ran", "check", "game", "shape", "equate", "hot", "miss", "brought", "heat", "snow", "tire", "bring", "yes", "distant", "fill", "east", "paint", "language", "among", "grand", "ball", "yet", "wave", "drop", "heart", "am", "present", "heavy", "dance", "engine", "position", "arm", "wide", "sail", "material", "size", "vary", "settle", "speak", "weight", "general", "ice", "matter", "circle", "pair", "include", "divide", "syllable", "felt", "perhaps", "pick", "sudden", "count", "square", "reason", "length", "represent", "art", "subject", "region", "energy", "hunt", "probable", "bed", "brother", "egg", "ride", "cell", "believe", "fraction", "forest", "sit", "race", "window", "store", "summer", "train", "sleep", "prove", "lone", "leg", "exercise", "wall", "catch", "mount", "wish", "sky", "board", "joy", "winter", "sat", "written", "wild", "instrument", "kept", "glass", "grass", "cow", "job", "edge", "sign", "visit", "past", "soft", "fun", "bright", "gas", "weather", "month", "million", "bear", "finish", "happy", "hope", "flower", "clothe", "strange", "gone", "jump", "baby", "eight", "village", "meet", "root", "buy", "raise", "solve", "metal", "whether", "push", "seven", "paragraph", "third", "shall", "held", "hair", "describe", "cook", "floor", "either", "result", "burn", "hill", "safe", "cat", "century", "consider", "type", "law", "bit", "coast", "copy", "phrase", "silent", "tall", "sand", "soil", "roll", "temperature", "finger", "industry", "value", "fight", "lie", "beat", "excite", "natural", "view", "sense", "ear", "else", "quite", "broke", "case", "middle", "kill", "son", "lake", "moment", "scale", "loud", "spring", "observe", "child", "straight", "consonant", "nation", "dictionary", "milk", "speed", "method", "organ", "pay", "age", "section", "dress", "cloud", "surprise", "quiet", "stone", "tiny", "climb", "cool", "design", "poor", "lot", "experiment", "bottom", "key", "iron", "single", "stick", "flat", "twenty", "skin", "smile", "crease", "hole", "trade", "melody", "trip", "office", "receive", "row", "mouth", "exact", "symbol", "die", "least", "trouble", "shout", "except", "wrote", "seed", "tone", "join", "suggest", "clean", "break", "lady", "yard", "rise", "bad", "blow", "oil", "blood", "touch", "grew", "cent", "mix", "team", "wire", "cost", "lost", "brown", "wear", "garden", "equal", "sent", "choose", "fell", "fit", "flow", "fair", "bank", "collect", "save", "control", "decimal", "gentle", "woman", "captain", "practice", "separate", "difficult", "doctor", "please", "protect", "noon", "whose", "locate", "ring", "character", "insect", "caught", "period", "indicate", "radio", "spoke", "atom", "human", "history", "effect", "electric", "expect", "crop", "modern", "element", "hit", "student", "corner", "party", "supply", "bone", "rail", "imagine", "provide", "agree", "thus", "capital", "chair", "danger", "fruit", "rich", "thick", "soldier", "process", "operate", "guess", "necessary", "sharp", "wing", "create", "neighbor", "wash", "bat", "rather", "crowd", "corn", "compare", "poem", "string", "bell", "depend", "meat", "rub", "tube", "famous", "dollar", "stream", "fear", "sight", "thin", "triangle", "planet", "hurry", "chief", "colony", "clock", "mine", "tie", "enter", "major", "fresh", "search", "send", "yellow", "gun", "allow", "print", "dead", "spot", "desert", "suit", "current", "lift", "rose", "continue", "block", "chart", "hat", "sell", "success", "company", "subtract", "event", "particular", "deal", "swim", "term", "opposite", "wife", "shoe", "shoulder", "spread", "arrange", "camp", "invent", "cotton", "born", "determine", "quart", "nine", "truck", "noise", "level", "chance", "gather", "shop", "stretch", "throw", "shine", "property", "column", "molecule", "select", "wrong", "gray", "repeat", "require", "broad", "prepare", "salt", "nose", "plural", "anger", "claim", "continent", "oxygen", "sugar", "death", "pretty", "skill", "women", "season", "solution", "magnet", "silver", "thank", "branch", "match", "suffix", "especially", "fig", "afraid", "huge", "sister", "steel", "discuss", "forward", "similar", "guide", "experience", "score", "apple", "bought", "led", "pitch", "coat", "mass", "card", "band", "rope", "slip", "win", "dream", "evening", "condition", "feed", "tool", "total", "basic", "smell", "valley", "nor", "double", "seat", "arrive", "master", "track", "parent", "shore", "division", "sheet", "substance", "favor", "connect", "post", "spend", "chord", "fat", "glad", "original", "share", "station", "dad", "bread", "charge", "proper", "bar", "offer", "segment", "slave", "duck", "instant", "market", "degree", "populate", "chick", "dear", "enemy", "reply", "drink", "occur", "support", "speech", "nature", "range", "steam", "motion", "path", "liquid", "log", "meant", "quotient", "teeth", "shell", "neck" },
                        null //null - do not check if adding was perform correctly
                    ),
                    new SearchTrieOp("Search for 'you' with dist=2", "you", 2,
                        new (string, int)[] { ( "box", 2 ), ( "boy", 2 ), ( "cow", 2 ), ( "do", 2 ), ( "dog", 2 ), ( "for", 2 ), ( "four", 2 ), ( "go", 2 ), ( "got", 2 ), ( "hot", 2 ), ( "hour", 2 ), ( "how", 2 ), ( "job", 2 ), ( "joy", 2 ), ( "log", 2 ), ( "lot", 2 ), ( "loud", 2 ), ( "low", 2 ), ( "no", 2 ), ( "nor", 2 ), ( "noun", 2 ), ( "now", 2 ), ( "of", 2 ), ( "oh", 2 ), ( "on", 2 ), ( "or", 2 ), ( "our", 2 ), ( "out", 2 ), ( "row", 2 ), ( "so", 2 ), ( "son", 2 ), ( "to", 2 ), ( "too", 2 ), ( "top", 2 ), ( "yes", 2 ), ( "yet", 2 ), ( "you", 0 ), ( "young", 2 ), ( "your", 1 ) }
                    ),
                    new SearchTrieOp("Search for 'make' with dist=2", "make", 2,
                        new (string, int)[] { ( "age", 2 ), ( "are", 2 ), ( "base", 2 ), ( "came", 2 ), ( "care", 2 ), ( "case", 2 ), ( "ease", 2 ), ( "face", 2 ), ( "game", 2 ), ( "gave", 2 ), ( "have", 2 ), ( "lake", 1 ), ( "late", 2 ), ( "like", 2 ), ( "made", 1 ), ( "main", 2 ), ( "make", 0 ), ( "man", 2 ), ( "many", 2 ), ( "map", 2 ), ( "mark", 2 ), ( "market", 2 ), ( "mass", 2 ), ( "may", 2 ), ( "me", 2 ), ( "mile", 2 ), ( "mine", 2 ), ( "more", 2 ), ( "move", 2 ), ( "name", 2 ), ( "page", 2 ), ( "race", 2 ), ( "safe", 2 ), ( "same", 2 ), ( "save", 2 ), ( "take", 1 ), ( "wave", 2 ) }
                    ),
                    new SearchTrieOp("Search for 'make' with dist=2", "make", 1,
                        new (string, int)[] { ( "lake", 1 ), ( "made", 1 ), ( "make", 0 ), ( "take", 1 ) }
                    ),
                    new SearchTrieOp("Search for 'word' with dist=2", "word", 3,
                        new (string, int)[] { ( "add", 3 ), ( "air", 3 ), ( "and", 3 ), ( "are", 3 ), ( "arm", 3 ), ( "art", 3 ), ( "bad", 3 ), ( "band", 3 ), ( "bar", 3 ), ( "bed", 3 ), ( "bird", 2 ), ( "blood", 3 ), ( "board", 2 ), ( "boat", 3 ), ( "body", 3 ), ( "bone", 3 ), ( "book", 3 ), ( "born", 2 ), ( "both", 3 ), ( "box", 3 ), ( "boy", 3 ), ( "broad", 3 ), ( "burn", 3 ), ( "car", 3 ), ( "card", 2 ), ( "care", 3 ), ( "chord", 2 ), ( "cloud", 3 ), ( "coat", 3 ), ( "cold", 2 ), ( "come", 3 ), ( "cook", 3 ), ( "cool", 3 ), ( "copy", 3 ), ( "corn", 2 ), ( "cost", 3 ), ( "could", 3 ), ( "cow", 3 ), ( "crowd", 3 ), ( "cry", 3 ), ( "dad", 3 ), ( "dark", 3 ), ( "dead", 3 ), ( "did", 3 ), ( "do", 3 ), ( "does", 3 ), ( "dog", 3 ), ( "done", 3 ), ( "door", 3 ), ( "down", 3 ), ( "dry", 3 ), ( "ear", 3 ), ( "end", 3 ), ( "far", 3 ), ( "farm", 3 ), ( "feed", 3 ), ( "find", 3 ), ( "fire", 3 ), ( "food", 2 ), ( "foot", 3 ), ( "for", 2 ), ( "force", 3 ), ( "form", 2 ), ( "found", 3 ), ( "four", 3 ), ( "girl", 3 ), ( "glad", 3 ), ( "go", 3 ), ( "gold", 2 ), ( "gone", 3 ), ( "good", 2 ), ( "got", 3 ), ( "had", 3 ), ( "hand", 3 ), ( "hard", 2 ), ( "head", 3 ), ( "heard", 3 ), ( "held", 3 ), ( "her", 3 ), ( "here", 3 ), ( "hold", 2 ), ( "hole", 3 ), ( "home", 3 ), ( "hope", 3 ), ( "horse", 3 ), ( "hot", 3 ), ( "hour", 3 ), ( "how", 3 ), ( "job", 3 ), ( "join", 3 ), ( "joy", 3 ), ( "kind", 3 ), ( "land", 3 ), ( "lead", 3 ), ( "led", 3 ), ( "log", 3 ), ( "lone", 3 ), ( "long", 3 ), ( "look", 3 ), ( "lost", 3 ), ( "lot", 3 ), ( "loud", 2 ), ( "love", 3 ), ( "low", 3 ), ( "mark", 3 ), ( "mind", 3 ), ( "moon", 3 ), ( "more", 2 ), ( "most", 3 ), ( "move", 3 ), ( "need", 3 ), ( "no", 3 ), ( "noon", 3 ), ( "nor", 2 ), ( "north", 3 ), ( "nose", 3 ), ( "note", 3 ), ( "noun", 3 ), ( "now", 3 ), ( "of", 3 ), ( "off", 3 ), ( "oh", 3 ), ( "oil", 3 ), ( "old", 2 ), ( "on", 3 ), ( "one", 3 ), ( "or", 2 ), ( "order", 3 ), ( "our", 3 ), ( "out", 3 ), ( "own", 3 ), ( "part", 3 ), ( "poem", 3 ), ( "poor", 3 ), ( "port", 2 ), ( "pose", 3 ), ( "post", 3 ), ( "pound", 3 ), ( "read", 3 ), ( "record", 3 ), ( "red", 3 ), ( "road", 2 ), ( "rock", 3 ), ( "roll", 3 ), ( "room", 3 ), ( "root", 3 ), ( "rope", 3 ), ( "rose", 3 ), ( "round", 3 ), ( "row", 3 ), ( "said", 3 ), ( "sand", 3 ), ( "score", 3 ), ( "seed", 3 ), ( "send", 3 ), ( "shore", 3 ), ( "short", 3 ), ( "so", 3 ), ( "soft", 3 ), ( "soil", 3 ), ( "some", 3 ), ( "son", 3 ), ( "song", 3 ), ( "soon", 3 ), ( "sound", 3 ), ( "stood", 3 ), ( "store", 3 ), ( "story", 3 ), ( "sure", 3 ), ( "term", 3 ), ( "third", 3 ), ( "tire", 3 ), ( "to", 3 ), ( "told", 2 ), ( "tone", 3 ), ( "too", 3 ), ( "took", 3 ), ( "tool", 3 ), ( "top", 3 ), ( "toward", 3 ), ( "town", 3 ), ( "try", 3 ), ( "turn", 3 ), ( "two", 3 ), ( "vary", 3 ), ( "verb", 3 ), ( "very", 3 ), ( "wait", 3 ), ( "walk", 3 ), ( "wall", 3 ), ( "want", 3 ), ( "war", 2 ), ( "warm", 2 ), ( "was", 3 ), ( "wash", 3 ), ( "wave", 3 ), ( "way", 3 ), ( "we", 3 ), ( "wear", 3 ), ( "week", 3 ), ( "well", 3 ), ( "went", 3 ), ( "were", 2 ), ( "west", 3 ), ( "what", 3 ), ( "when", 3 ), ( "where", 3 ), ( "who", 3 ), ( "whole", 3 ), ( "whose", 3 ), ( "why", 3 ), ( "wide", 3 ), ( "wife", 3 ), ( "wild", 2 ), ( "will", 3 ), ( "win", 3 ), ( "wind", 2 ), ( "wing", 3 ), ( "wire", 2 ), ( "wish", 3 ), ( "with", 3 ), ( "woman", 3 ), ( "women", 3 ), ( "wonder", 3 ), ( "wood", 1 ), ( "word", 0 ), ( "work", 1 ), ( "world", 1 ), ( "would", 2 ), ( "wrong", 3 ), ( "wrote", 3 ), ( "yard", 2 ), ( "you", 3 ), ( "your", 3 ) }
                    ),
                    new SearchTrieOp("Search for 'wall' with dist=2", "wall", 1,
                        new (string, int)[] { ( "all", 1 ), ( "ball", 1 ), ( "call", 1 ), ( "fall", 1 ), ( "tall", 1 ), ( "walk", 1 ), ( "wall", 0 ), ( "well", 1 ), ( "will", 1 ) }
                    ),
                    new SearchTrieOp("Search for 'question' with dist=2", "question", 3,
                        new (string, int)[] { ( "question", 0 ), ( "section", 3 ) }
                    )
                }));

            return searchTrieTestSet;
        }
    }

    class LabMain
    {
        static void Main(string[] args)
        {
            var tests = new TrieTester();
            tests.PrepareTestSets();
            foreach (var testSet in tests.TestSets.Values)
                testSet.PerformTests(false);
        }
    }
}
