using System;
using System.Collections.Generic;

namespace BankingApp.Data.Models;

public partial class Transaction
{
    public Guid Id { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; }

    public Guid? SavingAccountId { get; set; }

    public Guid? CheckingAccountId { get; set; }

    public Guid CustomerId { get; set; }

    public virtual CheckingAccount? CheckingAccount { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual SavingAccount? SavingAccount { get; set; }
}
