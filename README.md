# Bank Account Simulator

A C# console app simulating savings and checking accounts: deposits, withdrawals, a running transaction history, monthly interest accrual on savings, and overdraft handling (with a fee) on checking. Built as an OOP exercise — the kind of "model a real-world object hierarchy correctly" project that's a staple of junior C# interviews.

## Why this exists

My other languages are Java, JavaScript, Python, PHP and C++; this is my first real C# project. A bank account simulator is a deliberately classic choice — it's not trying to be original, it's trying to show inheritance, polymorphism, custom exceptions, and a clean separation between "what every account can do" and "what makes this account type different" done properly.

## What it does

- **`Account`** (abstract) — owner, balance, and transaction history. Handles deposits and basic withdrawals, and defines `AvailableBalance()` as a hook subclasses can override.
- **`SavingsAccount`** — earns monthly interest (`AccrueMonthlyInterest()`, annual rate ÷ 12, applied to the current balance) and has no overdraft: it relies on `Account`'s default behavior entirely, since a plain balance check is exactly what a savings account needs.
- **`CheckingAccount`** — overrides `AvailableBalance()` to include an overdraft limit, and overrides `Withdraw()` to charge a flat overdraft fee the moment a withdrawal actually dips the balance negative (draining to exactly zero doesn't count as overdrawing).
- **A REPL** (`src/BankAccountSimulator/Program.cs`) — `OPEN`, `DEPOSIT`, `WITHDRAW`, `ACCRUE`, `HISTORY`, `LIST`, `HELP`, `EXIT`.

## Design notes

- **Credit/Debit, not direct balance mutation.** `Account` exposes `protected` `Credit()`/`Debit()` methods that both update the balance and record a transaction in one place. `SavingsAccount.AccrueMonthlyInterest()` uses `Credit()` directly (not `Deposit()`) so interest shows up in the history as its own `TransactionType.InterestAccrual` instead of being indistinguishable from a regular deposit.
- **`AvailableBalance()` is the only thing that differs between account types for withdrawal eligibility.** `SavingsAccount` doesn't touch it (balance only); `CheckingAccount` adds the overdraft limit on top. `Account.Withdraw()` itself is written once, against `AvailableBalance()`, and only `CheckingAccount` needs to override `Withdraw()` at all — to layer the overdraft fee on top of the same base logic via `base.Withdraw(...)`.
- **Currency formatting and parsing are pinned to `en-US` explicitly** (`CultureInfo.GetCultureInfo("en-US")` for output, `CultureInfo.InvariantCulture` for parsing user input) rather than relying on the system's current culture. This was actually caught during testing: in this sandbox's minimal/invariant-globalization environment, `{amount:C}` silently printed the placeholder symbol `¤` instead of `$`. Pinning the culture explicitly means the output is identical regardless of what locale the machine running it happens to be configured for.

## Building and running

```bash
dotnet build
dotnet run --project src/BankAccountSimulator
```

```
Bank Account Simulator -- type HELP for commands.
> OPEN SAVINGS Maged 1200 0.12
Opened account #1: SAVINGS for Maged, balance $1,200.00
> ACCRUE 1
Accrued $12.00 interest. New balance: $1,212.00
> OPEN CHECKING Petra 100 50
Opened account #2: CHECKING for Petra, balance $100.00
> WITHDRAW 2 120
New balance: -$55.00
```

## Running the tests

```bash
dotnet run --project tests/BankAccountSimulator.Tests
```

20 tests against a small hand-rolled assertion runner (`tests/BankAccountSimulator.Tests/TestHarness.cs`) rather than xUnit — this was built in a sandbox that couldn't reach NuGet to install a testing framework (or, it turned out, to restore *any* package-based project at all; even a bare `dotnet new console` needs network access during restore unless pointed at a local-only package source). Rather than skip tests, these are real assertions that really execute: opening-balance validation, deposit/withdraw behavior and transaction recording, monthly interest math (including the zero-balance edge case), and the overdraft boundary — exactly-zero vs. actually negative, fee charged vs. not, and the hard limit where even the overdraft is exhausted.

## Project layout

```
src/BankAccountSimulator/
  Account.cs              Abstract base: balance, history, Deposit/Withdraw, AvailableBalance hook
  SavingsAccount.cs        Adds monthly interest accrual
  CheckingAccount.cs       Adds overdraft limit + fee
  Transaction.cs           Transaction record (type, amount, description, timestamp)
  TransactionType.cs       Deposit / Withdrawal / InterestAccrual / Fee
  Exceptions/
    InvalidAmountException.cs
    InsufficientFundsException.cs
  Program.cs                The REPL
tests/BankAccountSimulator.Tests/
  TestHarness.cs            The minimal assertion runner described above
  AccountTests.cs           Tests of the shared Account behavior
  SavingsAccountTests.cs
  CheckingAccountTests.cs
  Program.cs                 Registers and runs every test
```
