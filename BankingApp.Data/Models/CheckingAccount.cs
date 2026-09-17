using System;
using System.Collections.Generic;

namespace BankingApp.Data.Models;

public partial class CheckingAccount
{
    public Guid Id { get; set; }

    public string AccountNumber { get; set; } = null!;

    public string RoutingNumber { get; set; } = null!;

    public decimal Balance { get; set; }

    public DateTime DateOpen { get; set; }

    public string Status { get; set; } = null!;

    public Guid CustomerId { get; set; }

    public virtual ICollection<ChequeBookRequest> ChequeBookRequests { get; set; } = new List<ChequeBookRequest>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
