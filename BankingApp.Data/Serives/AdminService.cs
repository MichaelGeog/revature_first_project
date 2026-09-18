using BankingApp.Data.Models;
using System.Text.RegularExpressions;

namespace BankingApp.Data.Services
{
    public enum CreateCustomerResult
    {
        Success,
        InvalidInput,
        DuplicateUsername,
        DuplicateEmail,
        DuplicatePhoneNumber
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

        public bool? Login(string username, string password)
        {
            var admin = _dbContext.Admins.FirstOrDefault(a => a.Username == username);

            if (admin == null)
                return null;

            return BCrypt.Net.BCrypt.Verify(password, admin.Password);
        }

        public (CreateCustomerResult Result, Customer? Customer) CreateCustomer(string username, string password, string name, string phoneNumber, string email)
        {
            string digitsOnlyPhone = phoneNumber == null ? "" : Regex.Replace(phoneNumber, @"[^\d]", "");

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(phoneNumber) ||
                string.IsNullOrWhiteSpace(email) ||
                !email.Contains('@') ||
                digitsOnlyPhone.Length != 10)
            {
                return (CreateCustomerResult.InvalidInput, null);
            }

            if (_dbContext.Customers.Any(c => c.Username == username))
                return (CreateCustomerResult.DuplicateUsername, null);

            if (_dbContext.Customers.Any(c => c.Email == email))
                return (CreateCustomerResult.DuplicateEmail, null);

            if (_dbContext.Customers.Any(c => c.PhoneNumber == digitsOnlyPhone))
                return (CreateCustomerResult.DuplicatePhoneNumber, null);

            var newCustomer = new Customer
            {
                Username = username,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Name = name,
                PhoneNumber = digitsOnlyPhone,   // store cleaned digits, not raw input
                Email = email
            };

            _dbContext.Customers.Add(newCustomer);
            _dbContext.SaveChanges();

            return (CreateCustomerResult.Success, newCustomer);
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

    }
}