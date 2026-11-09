using BankAccountSimulator.Exceptions;

namespace BankAccountSimulator;

// A savings account earns interest but can never go negative -- it has no
// overdraft, so it relies entirely on Account's default AvailableBalance
// (just the balance) and default Withdraw behavior.
public class SavingsAccount : Account
{
    public decimal AnnualInterestRate { get; }

    public SavingsAccount(string owner, decimal openingBalance, decimal annualInterestRate)
        : base(owner, openingBalance)
    {
        if (annualInterestRate < 0)
        {
            throw new InvalidAmountException("Interest rate cannot be negative.");
        }
        AnnualInterestRate = annualInterestRate;
    }

    // Applies one month's worth of interest (annual rate / 12) to the
    // current balance, recorded as its own transaction type rather than a
    // plain deposit so the history shows where the money actually came
    // from. A zero balance accrues nothing and leaves no transaction.
    public decimal AccrueMonthlyInterest()
    {
        decimal interest = Math.Round(Balance * (AnnualInterestRate / 12m), 2, MidpointRounding.AwayFromZero);
        if (interest > 0)
        {
            Credit(interest, TransactionType.InterestAccrual, "Monthly interest");
        }
        return interest;
    }
}
