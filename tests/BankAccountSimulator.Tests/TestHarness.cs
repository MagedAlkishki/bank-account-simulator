namespace BankAccountSimulator.Tests;

// A deliberately small stand-in for a real .NET testing framework (xUnit,
// NUnit, MSTest...). This project was built in a sandbox that couldn't
// reach NuGet to install one, so rather than skip tests, this is a real
// assertion runner: every test registered below actually executes and
// actually has to pass. Run it with `dotnet run` like any other console app.
public static class TestHarness
{
    private static readonly List<(string Name, Action Fn)> Tests = new();

    public static void Add(string name, Action fn) => Tests.Add((name, fn));

    public static int RunAll()
    {
        int passed = 0, failed = 0;
        foreach (var (name, fn) in Tests)
        {
            try
            {
                fn();
                Console.WriteLine($"  [PASS] {name}");
                passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [FAIL] {name} - {ex.Message}");
                failed++;
            }
        }
        Console.WriteLine();
        Console.WriteLine($"{passed + failed} tests, {passed} passed, {failed} failed");
        return failed == 0 ? 0 : 1;
    }
}

public static class Assert
{
    public static void Equal<T>(T expected, T actual, string? message = null)
    {
        if (!Equals(expected, actual))
        {
            throw new Exception(message ?? $"Expected {expected}, got {actual}");
        }
    }

    public static void True(bool condition, string message = "Expected true")
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    public static void False(bool condition, string message = "Expected false")
    {
        if (condition)
        {
            throw new Exception(message);
        }
    }

    public static void Throws<TException>(Action action, string? message = null) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }
        catch (Exception ex)
        {
            throw new Exception(message ?? $"Expected {typeof(TException).Name}, got {ex.GetType().Name}");
        }
        throw new Exception(message ?? $"Expected {typeof(TException).Name} to be thrown, nothing was");
    }
}
