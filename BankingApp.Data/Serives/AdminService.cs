using BankingApp.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Data.Services
{
    public enum ExistingCustomerAccountStatus
    {
        HasActiveSaving,
        NeedsSavingAccount
    }
    public class AdminService
    {
        private const string BankRoutingNumber = "183555500";
        private readonly ProjectBankingAppContext _dbContext;
        private static readonly Random _random = new Random();
        public AdminService(ProjectBankingAppContext dbContext)
        {
            _dbContext = dbContext;
        }

        public (bool Found, bool PasswordValid, Admin? Admin) VerifyCredentials(string username, string password)
        {
            var admin = _dbContext.Admins.FirstOrDefault(a => a.Username == username);

            if (admin == null)
                return (false, false, null);

            bool passwordValid = BCrypt.Net.BCrypt.Verify(password, admin.Password);

            return (true, passwordValid, passwordValid ? admin : null);
        }

        public CheckingAccount CreateCheckingAccount(Guid customerId, decimal initialBalance)
        {
            string accountNumber;

            do
            {
                accountNumber = _random.Next(1_000_000_000, 2_000_000_000).ToString();
            }
            while (_dbContext.CheckingAccounts.Any(a => a.AccountNumber == accountNumber));

            var newAccount = new CheckingAccount
            {
                AccountNumber = accountNumber,
                RoutingNumber = BankRoutingNumber,
                Balance = initialBalance,
                DateOpen = DateTime.Now,
                Status = "Active",
                CustomerId = customerId
            };

            _dbContext.CheckingAccounts.Add(newAccount);
            _dbContext.SaveChanges();

            return newAccount;
        }

        public SavingAccount CreateSavingAccount(Guid customerId, decimal initialBalance)
        {
            string accountNumber;

            do
            {
                accountNumber = _random.Next(1_000_000_000, 2_000_000_000).ToString();
            }
            while (_dbContext.SavingAccounts.Any(a => a.AccountNumber == accountNumber));

            var newAccount = new SavingAccount
            {
                AccountNumber = accountNumber,
                RoutingNumber = BankRoutingNumber,
                Balance = initialBalance,
                DateOpen = DateTime.Now,
                Status = "Active",
                CustomerId = customerId
            };

            _dbContext.SavingAccounts.Add(newAccount);
            _dbContext.SaveChanges();

            return newAccount;
        }

        public ExistingCustomerAccountStatus CheckSavingAccountStatus(Guid customerId)
        {
            bool hasActiveSaving = _dbContext.SavingAccounts.Any(a => a.CustomerId == customerId && a.Status == "Active");
            return hasActiveSaving ? ExistingCustomerAccountStatus.HasActiveSaving : ExistingCustomerAccountStatus.NeedsSavingAccount;
        }

        public (bool HasActiveChecking, bool HasActiveSaving) GetActiveAccountsStatus(Guid customerId)
        {
            bool hasChecking = _dbContext.CheckingAccounts.Any(a => a.CustomerId == customerId && a.Status == "Active");
            bool hasSaving = _dbContext.SavingAccounts.Any(a => a.CustomerId == customerId && a.Status == "Active");

            return (hasChecking, hasSaving);
        }

        public void CloseCheckingAccount(Guid customerId)
        {
            var account = _dbContext.CheckingAccounts.FirstOrDefault(a => a.CustomerId == customerId && a.Status == "Active");

            if (account != null)
            {
                account.Status = "Closed";
                _dbContext.SaveChanges();
            }
        }

        public void CloseSavingAccount(Guid customerId)
        {
            var account = _dbContext.SavingAccounts.FirstOrDefault(a => a.CustomerId == customerId && a.Status == "Active");

            if (account != null)
            {
                account.Status = "Closed";
                _dbContext.SaveChanges();
            }
        }

        public void DisplaySummary()
        {
            int adminCount = _dbContext.Admins.Count();
            int customerCount = _dbContext.Customers.Count();

            int activeChecking = _dbContext.CheckingAccounts.Count(a => a.Status == "Active");
            int inactiveChecking = _dbContext.CheckingAccounts.Count(a => a.Status == "Inactive");
            int closedChecking = _dbContext.CheckingAccounts.Count(a => a.Status == "Closed");

            int activeSaving = _dbContext.SavingAccounts.Count(a => a.Status == "Active");
            int inactiveSaving = _dbContext.SavingAccounts.Count(a => a.Status == "Inactive");
            int closedSaving = _dbContext.SavingAccounts.Count(a => a.Status == "Closed");

            Console.WriteLine("\n===== Bank Summary =====");
            Console.WriteLine($"Total Admins: {adminCount}");
            Console.WriteLine($"Total Customers: {customerCount}");
            Console.WriteLine($"Active Checking Accounts: {activeChecking}");
            Console.WriteLine($"Inactive Checking Accounts: {inactiveChecking}");
            Console.WriteLine($"Closed Checking Accounts: {closedChecking}");
            Console.WriteLine($"Active Saving Accounts: {activeSaving}");
            Console.WriteLine($"Inactive Saving Accounts: {inactiveSaving}");
            Console.WriteLine($"Closed Saving Accounts: {closedSaving}");
            Console.WriteLine("=========================");
        }

        public List<ChequeBookRequest> GetPendingChequeBookRequests()
        {
            return _dbContext.ChequeBookRequests
                .Include(r => r.Customer)
                .Include(r => r.CheckingAccount)
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.RequestDate)
                .ToList();
        }

        public void ApproveChequeBookRequest(Guid requestId, Guid adminId)
        {
            var request = _dbContext.ChequeBookRequests.FirstOrDefault(r => r.Id == requestId);

            if (request != null)
            {
                request.Status = "Approved";
                request.AdminId = adminId;
                _dbContext.SaveChanges();
            }
        }

        public void DenyChequeBookRequest(Guid requestId, Guid adminId)
        {
            var request = _dbContext.ChequeBookRequests.FirstOrDefault(r => r.Id == requestId);

            if (request != null)
            {
                request.Status = "Rejected";
                request.AdminId = adminId;
                _dbContext.SaveChanges();
            }
        }

    }
}