using BankAccountSimulator.Exceptions;

namespace BankAccountSimulator;

// A checking account allows the balance to go negative up to an overdraft
// limit, and charges a flat fee the moment a withdrawal actually dips into
// that overdraft (draining the balance to exactly zero does not count).
public class CheckingAccount : Account
{
    public decimal OverdraftLimit { get; }
    public decimal OverdraftFee { get; }

    public CheckingAccount(string owner, decimal openingBalance, decimal overdraftLimit, decimal overdraftFee = 35m)
        : base(owner, openingBalance)
    {
        if (overdraftLimit < 0)
        {
            throw new InvalidAmountException("Overdraft limit cannot be negative.");
        }
        if (overdraftFee < 0)
        {
            throw new InvalidAmountException("Overdraft fee cannot be negative.");
        }

        OverdraftLimit = overdraftLimit;
        OverdraftFee = overdraftFee;
    }

    protected override decimal AvailableBalance() => Balance + OverdraftLimit;

    public override void Withdraw(decimal amount, string description = "Withdrawal")
    {
        bool willOverdraw = amount > Balance;
        base.Withdraw(amount, description);
        if (willOverdraw && OverdraftFee > 0)
        {
            Debit(OverdraftFee, TransactionType.Fee, "Overdraft fee");
        }
    }
}
