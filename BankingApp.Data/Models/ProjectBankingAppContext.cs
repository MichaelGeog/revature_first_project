using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Data.Models;

public partial class ProjectBankingAppContext : DbContext
{
    public ProjectBankingAppContext()
    {
    }

    public ProjectBankingAppContext(DbContextOptions<ProjectBankingAppContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<CheckingAccount> CheckingAccounts { get; set; }

    public virtual DbSet<ChequeBookRequest> ChequeBookRequests { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<SavingAccount> SavingAccounts { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    /* protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-O52VLJ1;Database=ProjectBankingApp;Trusted_Connection=True;TrustServerCertificate=True;");
 */
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Admins_Email").IsUnique();

            entity.HasIndex(e => e.PhoneNumber, "UQ_Admins_PhoneNumber").IsUnique();

            entity.HasIndex(e => e.Username, "UQ_Admins_Username").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())", "DF_Admins_Id")
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone_number");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<CheckingAccount>(entity =>
        {
            entity.HasIndex(e => e.AccountNumber, "UQ_CheckingAccounts_AccountNumber").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())", "DF_CheckingAccounts_Id")
                .HasColumnName("id");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("account_number");
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DateOpen)
                .HasDefaultValueSql("(sysdatetime())", "DF_CheckingAccounts_DateOpen")
                .HasColumnName("date_open");
            entity.Property(e => e.RoutingNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("routing_number");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active", "DF_CheckingAccounts_Status")
                .HasColumnName("status");

            entity.HasOne(d => d.Customer).WithMany(p => p.CheckingAccounts)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CheckingAccounts_Customers");
        });

        modelBuilder.Entity<ChequeBookRequest>(entity =>
        {
            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())", "DF_ChequeBookRequests_Id")
                .HasColumnName("id");
            entity.Property(e => e.AdminId).HasColumnName("admin_id");
            entity.Property(e => e.CheckingAccountId).HasColumnName("checking_account_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.RequestDate)
                .HasDefaultValueSql("(sysdatetime())", "DF_ChequeBookRequests_Date")
                .HasColumnName("request_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Pending", "DF_ChequeBookRequests_Status")
                .HasColumnName("status");

            entity.HasOne(d => d.Admin).WithMany(p => p.ChequeBookRequests)
                .HasForeignKey(d => d.AdminId)
                .HasConstraintName("FK_ChequeBookRequests_Admins");

            entity.HasOne(d => d.CheckingAccount).WithMany(p => p.ChequeBookRequests)
                .HasForeignKey(d => d.CheckingAccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChequeBookRequests_CheckingAccounts");

            entity.HasOne(d => d.Customer).WithMany(p => p.ChequeBookRequests)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ChequeBookRequests_Customers");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => e.Email, "UQ_Customers_Email").IsUnique();

            entity.HasIndex(e => e.PhoneNumber, "UQ_Customers_PhoneNumber").IsUnique();

            entity.HasIndex(e => e.Username, "UQ_Customers_Username").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())", "DF_Customers_Id")
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("phone_number");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("username");
        });

        modelBuilder.Entity<SavingAccount>(entity =>
        {
            entity.HasIndex(e => e.AccountNumber, "UQ_SavingAccounts_AccountNumber").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())", "DF_SavingAccounts_Id")
                .HasColumnName("id");
            entity.Property(e => e.AccountNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("account_number");
            entity.Property(e => e.Balance)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("balance");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.DateOpen)
                .HasDefaultValueSql("(sysdatetime())", "DF_SavingAccounts_DateOpen")
                .HasColumnName("date_open");
            entity.Property(e => e.InterestRate)
                .HasDefaultValue(0.0001m, "DF_SavingAccounts_InterestRate")
                .HasColumnType("decimal(5, 4)")
                .HasColumnName("interest_rate");
            entity.Property(e => e.RoutingNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("routing_number");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Active", "DF_SavingAccounts_Status")
                .HasColumnName("status");
            entity.Property(e => e.WithdrawalLimit)
                .HasDefaultValue(5000m, "DF_SavingAccounts_WithdrawlLimit")
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("withdrawal_limit");

            entity.HasOne(d => d.Customer).WithMany(p => p.SavingAccounts)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SavingAccounts_Customers");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())", "DF_Transactions_Id")
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.CheckingAccountId).HasColumnName("checking_account_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.SavingAccountId).HasColumnName("saving_account_id");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(sysdatetime())", "DF_Transactions_Date")
                .HasColumnName("transaction_date");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("transaction_type");

            entity.HasOne(d => d.CheckingAccount).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CheckingAccountId)
                .HasConstraintName("FK_Transactions_CheckingAccounts");

            entity.HasOne(d => d.Customer).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_Customers");

            entity.HasOne(d => d.SavingAccount).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.SavingAccountId)
                .HasConstraintName("FK_Transactions_SavingAccounts");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
