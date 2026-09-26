namespace TestTargets.ControlFlow;

public class ShortBranchExpansion
{
    // Przypadek brzegowy: W oryginalnym CIL instrukcja 'if' generuje krótką instrukcję skoku (np. brfalse.s).
    // Po wstrzyknięciu bloków licznika przed każdą z instrukcji wewnątrz bloku 'if',
    // dystans do celu przekracza 127 bajtów. Weaver musi przepisać 'brfalse.s' na 'brfalse'.
    public int ForceBranchExpansion(bool condition)
    {
        if (condition)
        {
            int sum = 0;
            sum += 1;
            sum += 2;
            sum += 3;
            sum += 4;
            sum += 5;
            sum += 6;
            sum += 7;
            sum += 8;
            sum += 9;
            sum += 10;
            return sum;
        }

        return 0;
    }
}