using System;
using System.Collections.Generic;

namespace BankingApp.Data.Models;

public partial class Customer
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<CheckingAccount> CheckingAccounts { get; set; } = new List<CheckingAccount>();

    public virtual ICollection<ChequeBookRequest> ChequeBookRequests { get; set; } = new List<ChequeBookRequest>();

    public virtual ICollection<SavingAccount> SavingAccounts { get; set; } = new List<SavingAccount>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
