namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string s1 = "xxxcx";
            string s2 = "cdbbdf";

            int n1 = s1.Length, n2 = s2.Length;
            var dl = new int[n1 + 1, n2 + 1];

            for (int i = 0; i < n1; i++)
                dl[i, 0] = 0;
            for (int i = 0; i < n2; i++)
                dl[0, i] = 0;

            for (int i = 1; i <= n1; i++)
                for (int j = 1; j <= n2; j++)
                    if (s1[i - 1] == s2[j - 1])
                        dl[i, j] = dl[i - 1, j - 1] + 1;
                    else
                        dl[i, j] = Math.Max(dl[i - 1, j], dl[i, j - 1]);

            Console.WriteLine(dl[n1, n2]);
        }
    }
}
