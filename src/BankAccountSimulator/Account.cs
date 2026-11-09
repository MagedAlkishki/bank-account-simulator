using BankAccountSimulator.Exceptions;

namespace BankAccountSimulator;

// The shared behavior every account type gets for free: deposits, basic
// withdrawals, and a running transaction history. SavingsAccount and
// CheckingAccount differ only in what "available to withdraw" means and
// what happens around a withdrawal -- see AvailableBalance() and the
// overridable Withdraw().
public abstract class Account
{
    private readonly List<Transaction> _transactions = new();

    public string Owner { get; }
    public decimal Balance { get; private set; }
    public IReadOnlyList<Transaction> Transactions => _transactions.AsReadOnly();

    protected Account(string owner, decimal openingBalance)
    {
        if (string.IsNullOrWhiteSpace(owner))
        {
            throw new ArgumentException("Owner name cannot be empty.", nameof(owner));
        }
        if (openingBalance < 0)
        {
            throw new InvalidAmountException("Opening balance cannot be negative.");
        }

        Owner = owner;
        if (openingBalance > 0)
        {
            Credit(openingBalance, TransactionType.Deposit, "Opening balance");
        }
    }

    public void Deposit(decimal amount, string description = "Deposit")
    {
        if (amount <= 0)
        {
            throw new InvalidAmountException("Deposit amount must be greater than zero.");
        }
        Credit(amount, TransactionType.Deposit, description);
    }

    public virtual void Withdraw(decimal amount, string description = "Withdrawal")
    {
        if (amount <= 0)
        {
            throw new InvalidAmountException("Withdrawal amount must be greater than zero.");
        }
        if (amount > AvailableBalance())
        {
            throw new InsufficientFundsException(
                $"Cannot withdraw {amount:C} from {Owner}'s account - available balance is {AvailableBalance():C}.");
        }
        Debit(amount, TransactionType.Withdrawal, description);
    }

    // What this account type actually allows withdrawing against. A plain
    // account (SavingsAccount) is just the balance; CheckingAccount adds
    // its overdraft limit on top.
    protected virtual decimal AvailableBalance() => Balance;

    protected void Credit(decimal amount, TransactionType type, string description)
    {
        Balance += amount;
        _transactions.Add(new Transaction(type, amount, description, DateTime.UtcNow));
    }

    protected void Debit(decimal amount, TransactionType type, string description)
    {
        Balance -= amount;
        _transactions.Add(new Transaction(type, -amount, description, DateTime.UtcNow));
    }
}
