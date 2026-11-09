using BankAccountSimulator;
using BankAccountSimulator.Exceptions;

namespace BankAccountSimulator.Tests;

public static class CheckingAccountTests
{
    public static void Register()
    {
        TestHarness.Add("a negative overdraft limit is rejected", () =>
        {
            Assert.Throws<InvalidAmountException>(() => new CheckingAccount("Maged", 100m, -50m));
        });

        TestHarness.Add("withdrawing within the balance does not touch the overdraft or charge a fee", () =>
        {
            var account = new CheckingAccount("Maged", 100m, 50m, overdraftFee: 35m);
            account.Withdraw(60m);
            Assert.Equal(40m, account.Balance);
        });

        TestHarness.Add("withdrawing into the overdraft succeeds up to the limit", () =>
        {
            var account = new CheckingAccount("Maged", 100m, 50m, overdraftFee: 0m);
            account.Withdraw(130m); // balance goes to -30, within the 50 limit
            Assert.Equal(-30m, account.Balance);
        });

        TestHarness.Add("withdrawing past the balance plus overdraft limit fails", () =>
        {
            var account = new CheckingAccount("Maged", 100m, 50m);
            Assert.Throws<InsufficientFundsException>(() => account.Withdraw(151m)); // would need -51, limit is -50
        });

        TestHarness.Add("a withdrawal that dips into the overdraft charges the overdraft fee", () =>
        {
            var account = new CheckingAccount("Maged", 100m, 50m, overdraftFee: 35m);
            account.Withdraw(120m); // dips 20 into overdraft
            Assert.Equal(-55m, account.Balance); // 100 - 120 = -20, then -35 fee = -55
            Assert.Equal(TransactionType.Fee, account.Transactions[^1].Type);
            Assert.Equal(-35m, account.Transactions[^1].Amount);
        });

        TestHarness.Add("a withdrawal that drains the balance to exactly zero does not charge a fee", () =>
        {
            var account = new CheckingAccount("Maged", 100m, 50m, overdraftFee: 35m);
            account.Withdraw(100m);
            Assert.Equal(0m, account.Balance);
            Assert.Equal(TransactionType.Withdrawal, account.Transactions[^1].Type);
        });
    }
}
