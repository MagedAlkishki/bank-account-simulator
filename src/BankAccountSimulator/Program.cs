using System.Globalization;
using BankAccountSimulator;
using BankAccountSimulator.Exceptions;

// Formatting and parsing both pin to en-US explicitly rather than the
// system's current culture: in a minimal/invariant-globalization
// environment, decimal.Parse and "{x:C}" can otherwise silently use "."
// vs "," differently, or print the placeholder currency symbol "¤"
// instead of "$", depending on what locale happens to be configured.
var usCulture = CultureInfo.GetCultureInfo("en-US");
string Money(decimal amount) => amount.ToString("C", usCulture);
decimal ParseAmount(string text) => decimal.Parse(text, NumberStyles.Number, CultureInfo.InvariantCulture);

var accounts = new Dictionary<int, Account>();
int nextId = 1;

Console.WriteLine("Bank Account Simulator -- type HELP for commands.");

while (true)
{
    Console.Write("> ");
    string? line = Console.ReadLine();
    if (line is null)
    {
        break;
    }

    var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
    if (parts.Length == 0)
    {
        continue;
    }

    string command = parts[0].ToUpperInvariant();
    try
    {
        switch (command)
        {
            case "OPEN":
                HandleOpen(parts);
                break;
            case "DEPOSIT":
                HandleDeposit(parts);
                break;
            case "WITHDRAW":
                HandleWithdraw(parts);
                break;
            case "ACCRUE":
                HandleAccrue(parts);
                break;
            case "HISTORY":
                HandleHistory(parts);
                break;
            case "LIST":
                HandleList();
                break;
            case "HELP":
                PrintHelp();
                break;
            case "EXIT":
            case "QUIT":
                return 0;
            default:
                Console.WriteLine("Unknown command. Type HELP.");
                break;
        }
    }
    catch (Exception ex) when (ex is InvalidAmountException or InsufficientFundsException or ArgumentException or FormatException)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
}

return 0;

void HandleOpen(string[] parts)
{
    if (parts.Length < 4)
    {
        Console.WriteLine("Usage: OPEN SAVINGS|CHECKING <owner> <balance> [rate|overdraft]");
        return;
    }

    string type = parts[1].ToUpperInvariant();
    string owner = parts[2];
    decimal balance = ParseAmount(parts[3]);

    Account account = type switch
    {
        "SAVINGS" => new SavingsAccount(owner, balance, parts.Length > 4 ? ParseAmount(parts[4]) : 0.02m),
        "CHECKING" => new CheckingAccount(owner, balance, parts.Length > 4 ? ParseAmount(parts[4]) : 100m),
        _ => throw new ArgumentException($"Unknown account type '{type}'. Use SAVINGS or CHECKING.")
    };

    int id = nextId++;
    accounts[id] = account;
    Console.WriteLine($"Opened account #{id}: {type} for {owner}, balance {Money(account.Balance)}");
}

void HandleDeposit(string[] parts)
{
    if (parts.Length < 3)
    {
        Console.WriteLine("Usage: DEPOSIT <id> <amount>");
        return;
    }
    Account? account = GetAccount(parts[1]);
    if (account is null)
    {
        return;
    }
    account.Deposit(ParseAmount(parts[2]));
    Console.WriteLine($"New balance: {Money(account.Balance)}");
}

void HandleWithdraw(string[] parts)
{
    if (parts.Length < 3)
    {
        Console.WriteLine("Usage: WITHDRAW <id> <amount>");
        return;
    }
    Account? account = GetAccount(parts[1]);
    if (account is null)
    {
        return;
    }
    account.Withdraw(ParseAmount(parts[2]));
    Console.WriteLine($"New balance: {Money(account.Balance)}");
}

void HandleAccrue(string[] parts)
{
    if (parts.Length < 2)
    {
        Console.WriteLine("Usage: ACCRUE <id>");
        return;
    }
    Account? account = GetAccount(parts[1]);
    if (account is null)
    {
        return;
    }
    if (account is not SavingsAccount savings)
    {
        Console.WriteLine("Only savings accounts accrue interest.");
        return;
    }
    decimal interest = savings.AccrueMonthlyInterest();
    Console.WriteLine($"Accrued {Money(interest)} interest. New balance: {Money(savings.Balance)}");
}

void HandleHistory(string[] parts)
{
    if (parts.Length < 2)
    {
        Console.WriteLine("Usage: HISTORY <id>");
        return;
    }
    Account? account = GetAccount(parts[1]);
    if (account is null)
    {
        return;
    }
    foreach (var t in account.Transactions)
    {
        Console.WriteLine($"  [{t.Timestamp:u}] {t.Type,-16} {Money(t.Amount),10}  {t.Description}");
    }
}

void HandleList()
{
    foreach (var (id, account) in accounts)
    {
        Console.WriteLine($"#{id}: {account.GetType().Name,-16} {account.Owner,-15} {Money(account.Balance)}");
    }
}

Account? GetAccount(string idText)
{
    if (!int.TryParse(idText, out int id) || !accounts.TryGetValue(id, out var account))
    {
        Console.WriteLine($"No account #{idText}.");
        return null;
    }
    return account;
}

void PrintHelp()
{
    Console.WriteLine("""
        Commands:
          OPEN SAVINGS <owner> <balance> [annual_rate]
          OPEN CHECKING <owner> <balance> [overdraft_limit]
          DEPOSIT <id> <amount>
          WITHDRAW <id> <amount>
          ACCRUE <id>        (savings only -- applies one month's interest)
          HISTORY <id>
          LIST
          HELP
          EXIT
        """);
}
