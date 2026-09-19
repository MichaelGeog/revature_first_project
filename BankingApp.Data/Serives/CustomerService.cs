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

    public enum UpdateCustomerResult
    {
        Success,
        InvalidInput,
        DuplicateEmail,
        DuplicatePhoneNumber,
        CustomerNotFound
    }

    public enum WithdrawResult
    {
        Success,
        NoActiveAccount,
        InsufficientFunds,
        InvalidAmount
    }

    public enum TransactionType
    {
        Withdraw,
        Deposit,
        Transfer
    }

    public class CustomerService
    {
        private readonly ProjectBankingAppContext _dbContext;

        public CustomerService(ProjectBankingAppContext dbContext)
        {
            _dbContext = dbContext;
        }

        public bool? Login(string username, string password)
        {
            var customer = _dbContext.Customers.FirstOrDefault(c => c.Username == username);

            if (customer == null)
                return null;

            return BCrypt.Net.BCrypt.Verify(password, customer.Password);
        }

        public (bool Found, bool PasswordValid, Customer? Customer) VerifyCredentials(string username, string password)
        {
            var customer = _dbContext.Customers.FirstOrDefault(c => c.Username == username);

            if (customer == null)
                return (false, false, null);

            bool passwordValid = BCrypt.Net.BCrypt.Verify(password, customer.Password);

            return (true, passwordValid, passwordValid ? customer : null);
        }
        
        public (bool Verified, Customer? Customer) VerifyUsername(string username)
        {
            var customer = _dbContext.Customers.FirstOrDefault(c => c.Username == username);

            if (customer == null)
                return (false, null);

            return (true, customer);
        }

        public void ChangeCustomerPass(Customer Customer, string password)
        {
            Customer.Password = BCrypt.Net.BCrypt.HashPassword(password);
            _dbContext.SaveChanges();
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
                PhoneNumber = digitsOnlyPhone,
                Email = email
            };

            _dbContext.Customers.Add(newCustomer);
            _dbContext.SaveChanges();

            return (CreateCustomerResult.Success, newCustomer);
        }

        public UpdateCustomerResult UpdateCustomerName(Guid customerId, string name)
        {
            var customer = _dbContext.Customers.FirstOrDefault(c => c.Id == customerId);

            if (customer == null)
                return UpdateCustomerResult.CustomerNotFound;

            if (string.IsNullOrWhiteSpace(name))
                return UpdateCustomerResult.InvalidInput;

            customer.Name = name;
            _dbContext.SaveChanges();

            return UpdateCustomerResult.Success;
        }

        public UpdateCustomerResult UpdateCustomerPhone(Guid customerId, string phoneNumber)
        {
            var customer = _dbContext.Customers.FirstOrDefault(c => c.Id == customerId);

            if (customer == null)
                return UpdateCustomerResult.CustomerNotFound;

            string digitsOnlyPhone = phoneNumber == null ? "" : Regex.Replace(phoneNumber, @"[^\d]", "");

            if (digitsOnlyPhone.Length != 10)
                return UpdateCustomerResult.InvalidInput;

            if (_dbContext.Customers.Any(c => c.PhoneNumber == digitsOnlyPhone && c.Id != customerId))
                return UpdateCustomerResult.DuplicatePhoneNumber;

            customer.PhoneNumber = digitsOnlyPhone;
            _dbContext.SaveChanges();

            return UpdateCustomerResult.Success;
        }

        public UpdateCustomerResult UpdateCustomerEmail(Guid customerId, string email)
        {
            var customer = _dbContext.Customers.FirstOrDefault(c => c.Id == customerId);

            if (customer == null)
                return UpdateCustomerResult.CustomerNotFound;

            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                return UpdateCustomerResult.InvalidInput;

            if (_dbContext.Customers.Any(c => c.Email == email && c.Id != customerId))
                return UpdateCustomerResult.DuplicateEmail;

            customer.Email = email;
            _dbContext.SaveChanges();

            return UpdateCustomerResult.Success;
        }

        public CheckingAccount? GetActiveCheckingAccount(Guid customerId)
        {
            return _dbContext.CheckingAccounts.FirstOrDefault(a => a.CustomerId == customerId && a.Status == "Active");
        }

        public SavingAccount? GetActiveSavingAccount(Guid customerId)
        {
            return _dbContext.SavingAccounts.FirstOrDefault(a => a.CustomerId == customerId && a.Status == "Active");
        }

        public (bool HasActiveChecking, bool HasActiveSaving) GetActiveAccountsStatus(Guid customerId)
        {
            bool hasChecking = _dbContext.CheckingAccounts.Any(a => a.CustomerId == customerId && a.Status == "Active");
            bool hasSaving = _dbContext.SavingAccounts.Any(a => a.CustomerId == customerId && a.Status == "Active");

            return (hasChecking, hasSaving);
        }

        public (WithdrawResult Result, decimal NewBalance) Withdraw(Guid customerId, decimal withdrawAmount)
        {
            if (withdrawAmount <= 0)
                return (WithdrawResult.InvalidAmount, 0);

            var checkingAccount = GetActiveCheckingAccount(customerId);

            if (checkingAccount == null)
                return (WithdrawResult.NoActiveAccount, 0);

            if (checkingAccount.Balance < withdrawAmount)
                return (WithdrawResult.InsufficientFunds, checkingAccount.Balance);

            checkingAccount.Balance -= withdrawAmount;
            var newTransaction = new Transaction
            {
                TransactionType = TransactionType.Withdraw.ToString(),
                Amount = withdrawAmount,
                TransactionDate = DateTime.Now,
                CheckingAccountId = checkingAccount.Id,
                CustomerId = customerId
            };
            _dbContext.Transactions.Add(newTransaction);
            _dbContext.SaveChanges();

            return (WithdrawResult.Success, checkingAccount.Balance);
        }
    }
}