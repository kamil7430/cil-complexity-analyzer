using System;
using CilComplexityAnalyzer.Contract;
using CilComplexityAnalyzer.Contract.Attributes;
using NewTester.Lab02.Submissions;

namespace NewTester.Lab02;

//[TestSuite]
[StudentSolution(typeof(Submissions.Lab02))]
public partial class Lab02Evaluation : TestSuite
{
    public override TestSuiteSettings? Settings()
        => new TestSuiteSettings
        {
            Containerized = false,
        };

    public class Case0_Stage1_Przyklad : TestCase
    {
        private int _H;
        private int _W;
        private int[,] _S = null!;
        private int _expectedCost;
        private (int returnedCost, (int i, int j)[] seam) _result;

        public override int TestNumber() => 0;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _H = 5;
            _W = 5;
            _expectedCost = 8;
            _S = new int[5, 5]
            {
                {3, 2, 1, 3, 7},
                {6, 1, 8, 2, 7},
                {5, 9, 3, 9, 9},
                {4, 4, 1, 5, 6},
                {7, 2, 3, 8, 1}
            };
        }

        public override void Act()
        {
            var studentLab = new Submissions.Lab02();
            _result = studentLab.Stage1(_S);
        }

        public override void Assert()
        {
            CheckSeamAndCost(this, _H, _W, _S, _expectedCost, _result, K: null);
            base.Assert();
        }
    }

    public class Case1_Stage1_Minimalny : TestCase
    {
        private int _H;
        private int _W;
        private int[,] _S = null!;
        private int _expectedCost;
        private (int returnedCost, (int i, int j)[] seam) _result;

        public override int TestNumber() => 1;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _H = 2;
            _W = 2;
            _expectedCost = 4;
            _S = new int[2, 2]
            {
                {1, 2},
                {3, 4}
            };
        }

        public override void Act()
        {
            var studentLab = new Submissions.Lab02();
            _result = studentLab.Stage1(_S);
        }

        public override void Assert()
        {
            CheckSeamAndCost(this, _H, _W, _S, _expectedCost, _result, K: null);
            base.Assert();
        }
    }

    public class Case2_Stage1_Jednorodny : TestCase
    {
        private int _H;
        private int _W;
        private int[,] _S = null!;
        private int _expectedCost;
        private (int returnedCost, (int i, int j)[] seam) _result;

        public override int TestNumber() => 2;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _W = 100;
            _H = 2 * _W;
            _expectedCost = _H * 5;
            _S = MatrixHelper.CreateHomogeneousMatrix(_W, 5);
        }

        public override void Act()
        {
            var studentLab = new Submissions.Lab02();
            _result = studentLab.Stage1(_S);
        }

        public override void Assert()
        {
            CheckSeamAndCost(this, _H, _W, _S, _expectedCost, _result, K: null);
            base.Assert();
        }
    }

    public class Case3_Stage1_Gradientowy : TestCase
    {
        private int _H;
        private int _W;
        private int[,] _S = null!;
        private int _expectedCost;
        private (int returnedCost, (int i, int j)[] seam) _result;

        public override int TestNumber() => 3;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _W = 500;
            _H = 2 * _W;
            int baseValue = 100, stepRow = 1, stepCol = 2;
            _S = MatrixHelper.CreateGradientMatrix(_W, baseValue, stepRow, stepCol);
            _expectedCost = baseValue * _H + stepRow * (_H - 1) * _H / 2;
        }

        public override void Act()
        {
            var studentLab = new Submissions.Lab02();
            _result = studentLab.Stage1(_S);
        }

        public override void Assert()
        {
            CheckSeamAndCost(this, _H, _W, _S, _expectedCost, _result, K: null);
            base.Assert();
        }
    }

    public class Case4_Stage2_Przyklad : TestCase
    {
        private int _H;
        private int _W;
        private int _K;
        private int[,] _S = null!;
        private int _expectedCost;
        private (int returnedCost, (int i, int j)[] seam) _result;

        public override int TestNumber() => 4;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _H = 5;
            _W = 5;
            _K = 2;
            _expectedCost = 13;
            _S = new int[5, 5]
            {
                {3, 2, 1, 3, 7},
                {6, 1, 8, 2, 7},
                {5, 9, 3, 9, 9},
                {4, 4, 1, 5, 6},
                {7, 2, 3, 8, 1}
            };
        }

        public override void Act()
        {
            var studentLab = new Submissions.Lab02();
            _result = studentLab.Stage2(_S, _K);
        }

        public override void Assert()
        {
            CheckSeamAndCost(this, _H, _W, _S, _expectedCost, _result, _K);
            base.Assert();
        }
    }

    public class Case5_Stage2_Wredny1 : TestCase
    {
        private int _H;
        private int _W;
        private int _K;
        private int[,] _S = null!;
        private int _expectedCost;
        private (int returnedCost, (int i, int j)[] seam) _result;

        public override int TestNumber() => 5;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _H = 5;
            _W = 5;
            _K = 2;
            _expectedCost = 5;
            _S = new int[5, 5]
            {
                {1, 999, 999, 999, 1},
                {999, 1, 999, 1, 999},
                {999, 999, 1, 999, 999},
                {999, 1, 999, 1, 999},
                {2, 999, 999, 999, 1}
            };
        }

        public override void Act()
        {
            var studentLab = new Submissions.Lab02();
            _result = studentLab.Stage2(_S, _K);
        }

        public override void Assert()
        {
            CheckSeamAndCost(this, _H, _W, _S, _expectedCost, _result, _K);
            base.Assert();
        }
    }

    public class Case6_Stage2_LosowyDuzy : TestCase
    {
        private int _H;
        private int _W;
        private int _K;
        private int[,] _S = null!;
        private int _expectedCost;
        private (int returnedCost, (int i, int j)[] seam) _result;

        public override int TestNumber() => 6;

        public override TestCaseSettings Settings() => new TestCaseSettings();

        public override void Arrange()
        {
            _W = 10 * 16 * 8;
            _H = 15 * 16 * 8;
            _K = 3;
            _expectedCost = 6623;

            var rand = new Random(2025);
            _S = MatrixHelper.CreateRandomMatrix(_W, _H, 1, 10, rand);
        }

        public override void Act()
        {
            var studentLab = new Submissions.Lab02();
            _result = studentLab.Stage2(_S, _K);
        }

        public override void Assert()
        {
            CheckSeamAndCost(this, _H, _W, _S, _expectedCost, _result, _K);
            base.Assert();
        }
    }

    private static void CheckSeamAndCost(
        TestCase testCase,
        int H,
        int W,
        int[,] S,
        int expectedCost,
        (int returnedCost, (int i, int j)[] seam) result,
        int? K)
    {
        testCase.IsTrue(result.returnedCost == expectedCost,
            $"Zwrócono wartość kosztu {result.returnedCost}, a powinno być {expectedCost}");

        testCase.IsTrue(result.seam != null, "Zwrócono null zamiast ścieżki");
        if (result.seam == null)
            return;

        testCase.IsTrue(result.seam.Length == H,
            $"Zwrócona ścieżka ma długość {result.seam.Length}, powinna mieć {H}");

        if (result.seam.Length == 0)
            return;

        testCase.IsTrue(result.seam[0].i == 0, "Ścieżka nie zaczyna się w wierszu 0");
        testCase.IsTrue(result.seam[result.seam.Length - 1].i == H - 1, "Ścieżka nie kończy się w ostatnim wierszu");

        (int i, int j) pos = result.seam[0];
        for (int idx = 1; idx < result.seam.Length; idx++)
        {
            var curr = result.seam[idx];
            testCase.IsTrue(curr.i - pos.i == 1,
                $"Różnica wierszy między krokami nie wynosi 1: {pos} -> {curr}");

            int dj = curr.j - pos.j;
            testCase.IsTrue(dj >= -1 && dj <= 1,
                $"Różnica kolumn między krokami powinna wynosić -1, 0 lub 1: {pos} -> {curr}");
            testCase.IsTrue(curr.j >= 0 && curr.j < W,
                $"Krok wychodzi poza planszę: {curr}");

            pos = curr;
        }

        int basicCost = 0;
        foreach (var p in result.seam)
            basicCost += S[p.i, p.j];

        int penalty = 0;
        if (K.HasValue && result.seam.Length >= 2)
        {
            int prevDj = result.seam[1].j - result.seam[0].j;
            for (int idx = 2; idx < result.seam.Length; idx++)
            {
                int currDj = result.seam[idx].j - result.seam[idx - 1].j;
                if (currDj != prevDj)
                    penalty += K.Value;
                prevDj = currDj;
            }
        }

        int totalCost = basicCost + penalty;
        testCase.IsTrue(totalCost == result.returnedCost,
            $"Obliczony koszt (suma wartości pikseli + kara {penalty}) wynosi {totalCost}, a zwrócony koszt to {result.returnedCost}");
    }
}

public static class MatrixHelper
{
    public static int[,] CreateHomogeneousMatrix(int W, int value)
    {
        int H = 2 * W;
        int[,] matrix = new int[H, W];
        for (int i = 0; i < H; i++)
        for (int j = 0; j < W; j++)
            matrix[i, j] = value;
        return matrix;
    }

    public static int[,] CreateGradientMatrix(int W, int baseValue, int stepRow, int stepCol)
    {
        int H = 2 * W;
        int[,] matrix = new int[H, W];
        for (int i = 0; i < H; i++)
        for (int j = 0; j < W; j++)
            matrix[i, j] = baseValue + i * stepRow + j * stepCol;
        return matrix;
    }

    public static int[,] CreateRandomMatrix(int W, int H, int losMin, int losMax, Random rand)
    {
        int[,] matrix = new int[H, W];
        for (int i = 0; i < H; i++)
        for (int j = 0; j < W; j++)
            matrix[i, j] = rand.Next(losMin, losMax + 1);
        return matrix;
    }
}