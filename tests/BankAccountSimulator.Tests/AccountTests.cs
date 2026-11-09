using BankAccountSimulator;
using BankAccountSimulator.Exceptions;

namespace BankAccountSimulator.Tests;

// Exercises Account's shared behavior (deposit, withdraw, transaction
// history, opening-balance validation) through SavingsAccount, since
// SavingsAccount doesn't override Withdraw -- these are really tests of
// the base class.
public static class AccountTests
{
    public static void Register()
    {
        TestHarness.Add("opening an account with a positive balance records it as a transaction", () =>
        {
            var account = new SavingsAccount("Maged", 100m, 0.02m);
            Assert.Equal(100m, account.Balance);
            Assert.Equal(1, account.Transactions.Count);
            Assert.Equal(TransactionType.Deposit, account.Transactions[0].Type);
        });

        TestHarness.Add("opening an account with a zero balance records no transaction", () =>
        {
            var account = new SavingsAccount("Maged", 0m, 0.02m);
            Assert.Equal(0, account.Transactions.Count);
        });

        TestHarness.Add("opening an account with a negative balance throws", () =>
        {
            Assert.Throws<InvalidAmountException>(() => new SavingsAccount("Maged", -10m, 0.02m));
        });

        TestHarness.Add("opening an account with a blank owner throws", () =>
        {
            Assert.Throws<ArgumentException>(() => new SavingsAccount("   ", 100m, 0.02m));
        });

        TestHarness.Add("deposit increases the balance and records a transaction", () =>
        {
            var account = new SavingsAccount("Maged", 100m, 0.02m);
            account.Deposit(50m, "Paycheck");
            Assert.Equal(150m, account.Balance);
            Assert.Equal(2, account.Transactions.Count);
            Assert.Equal("Paycheck", account.Transactions[1].Description);
        });

        TestHarness.Add("deposit rejects a zero or negative amount", () =>
        {
            var account = new SavingsAccount("Maged", 100m, 0.02m);
            Assert.Throws<InvalidAmountException>(() => account.Deposit(0m));
            Assert.Throws<InvalidAmountException>(() => account.Deposit(-5m));
        });

        TestHarness.Add("withdraw decreases the balance and records a negative-amount transaction", () =>
        {
            var account = new SavingsAccount("Maged", 100m, 0.02m);
            account.Withdraw(40m, "Groceries");
            Assert.Equal(60m, account.Balance);
            Assert.Equal(-40m, account.Transactions[^1].Amount);
        });

        TestHarness.Add("withdraw rejects an amount greater than the balance", () =>
        {
            var account = new SavingsAccount("Maged", 100m, 0.02m);
            Assert.Throws<InsufficientFundsException>(() => account.Withdraw(150m));
            Assert.Equal(100m, account.Balance);
        });

        TestHarness.Add("withdraw rejects a zero or negative amount", () =>
        {
            var account = new SavingsAccount("Maged", 100m, 0.02m);
            Assert.Throws<InvalidAmountException>(() => account.Withdraw(0m));
        });
    }
}
