using BankAccountSimulator;
using BankAccountSimulator.Exceptions;

namespace BankAccountSimulator.Tests;

public static class SavingsAccountTests
{
    public static void Register()
    {
        TestHarness.Add("a negative interest rate is rejected", () =>
        {
            Assert.Throws<InvalidAmountException>(() => new SavingsAccount("Maged", 100m, -0.01m));
        });

        TestHarness.Add("monthly interest is the annual rate divided by twelve, applied to the balance", () =>
        {
            var account = new SavingsAccount("Maged", 1200m, 0.12m); // 12% annual -> 1% monthly
            decimal interest = account.AccrueMonthlyInterest();
            Assert.Equal(12.00m, interest);
            Assert.Equal(1212.00m, account.Balance);
        });

        TestHarness.Add("accrued interest is recorded as its own transaction type, not a plain deposit", () =>
        {
            var account = new SavingsAccount("Maged", 1200m, 0.12m);
            account.AccrueMonthlyInterest();
            Assert.Equal(TransactionType.InterestAccrual, account.Transactions[^1].Type);
        });

        TestHarness.Add("a zero balance accrues no interest and records no transaction", () =>
        {
            var account = new SavingsAccount("Maged", 0m, 0.12m);
            decimal interest = account.AccrueMonthlyInterest();
            Assert.Equal(0m, interest);
            Assert.Equal(0, account.Transactions.Count);
        });

        TestHarness.Add("a savings account has no overdraft -- withdrawing past the balance always fails", () =>
        {
            var account = new SavingsAccount("Maged", 50m, 0.02m);
            Assert.Throws<InsufficientFundsException>(() => account.Withdraw(50.01m));
        });
    }
}
