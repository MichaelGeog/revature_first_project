using System;
using System.Collections.Generic;

namespace BankingApp.Data.Models;

public partial class Admin
{
    public Guid Id { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual ICollection<ChequeBookRequest> ChequeBookRequests { get; set; } = new List<ChequeBookRequest>();
}
