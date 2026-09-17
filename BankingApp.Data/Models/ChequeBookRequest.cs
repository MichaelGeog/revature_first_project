using System;
using System.Collections.Generic;

namespace BankingApp.Data.Models;

public partial class ChequeBookRequest
{
    public Guid Id { get; set; }

    public DateTime RequestDate { get; set; }

    public string Status { get; set; } = null!;

    public Guid CheckingAccountId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid? AdminId { get; set; }

    public virtual Admin? Admin { get; set; }

    public virtual CheckingAccount CheckingAccount { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;
}
