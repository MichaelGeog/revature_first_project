using System;
using System.Collections.Generic;

namespace BankingApp.Data.Models;

public partial class SavingAccount
{
    public Guid Id { get; set; }

    public string AccountNumber { get; set; } = null!;

    public string RoutingNumber { get; set; } = null!;

    public decimal Balance { get; set; }

    public decimal InterestRate { get; set; }

    public decimal WithdrawalLimit { get; set; }

    public DateTime DateOpen { get; set; }

    public string Status { get; set; } = null!;

    public Guid CustomerId { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
