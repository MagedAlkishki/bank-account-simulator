using BankAccountSimulator.Tests;

AccountTests.Register();
SavingsAccountTests.Register();
CheckingAccountTests.Register();

return TestHarness.RunAll();
