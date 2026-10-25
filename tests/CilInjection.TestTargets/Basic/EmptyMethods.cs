namespace TestTargets.Basic;

public class EmptyMethods
{
    // Domyślny konstruktor (wywołuje base..ctor() + ret)
    public EmptyMethods()
    {
    }

    // Pusta metoda void (tylko ret)
    public void DoNothing()
    {
    }

    // Pusta statyczna metoda void (tylko ret)
    public static void StaticDoNothing()
    {
    }

    // Metoda zwracająca stałą wartość bez zmiennych lokalnych
    public int ReturnConstant()
    {
        return 42;
    }
}