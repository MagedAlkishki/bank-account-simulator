namespace BankAccountSimulator;

public record Transaction(TransactionType Type, decimal Amount, string Description, DateTime Timestamp);
