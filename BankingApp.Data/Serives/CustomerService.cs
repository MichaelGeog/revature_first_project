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

    public enum DepositResult
    {
        Success,
        NoActiveAccount,
        InvalidAmount
    }

    public enum TransferResult
    {
        Success,
        NoActiveAccount,
        InsufficientFunds,
        InvalidAmount,
        NoRecipientAccountFound,
        NoActiveRecipientCheckingAccount
    }

    public enum TransactionType
    {
        Withdraw,
        Deposit,
        Transfer
    }

    public enum RequestChequeBookResult
    {
        Success,
        NoActiveCheckingAccount,
        AlreadyPending
    }

    public enum ChangePasswordResult
    {
        Success,
        IncorrectCurrentPassword,
        InvalidNewPassword
    }

    public class CustomerService
    {
        private readonly ProjectBankingAppContext _dbContext;

        public bool UsernameExists(string username) => _dbContext.Customers.Any(c => c.Username == username);
        public bool EmailExists(string email) => _dbContext.Customers.Any(c => c.Email == email);
        public bool PhoneNumberExists(string digitsOnlyPhone) => _dbContext.Customers.Any(c => c.PhoneNumber == digitsOnlyPhone);

        public CustomerService(ProjectBankingAppContext dbContext)
        {
            _dbContext = dbContext;
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
                Amount = -withdrawAmount,
                TransactionDate = DateTime.Now,
                CheckingAccountId = checkingAccount.Id,
                CustomerId = customerId
            };
            _dbContext.Transactions.Add(newTransaction);
            _dbContext.SaveChanges();

            return (WithdrawResult.Success, checkingAccount.Balance);
        }
    
        public (DepositResult Result, decimal NewBalance) DepositToChecking(Guid customerId, decimal depositAmount)
        {
            if (depositAmount <= 0)
                return (DepositResult.InvalidAmount, 0);
            
            var checkingAccount = GetActiveCheckingAccount(customerId);

            if (checkingAccount == null)
                return (DepositResult.NoActiveAccount, 0);
            
            checkingAccount.Balance += depositAmount;

            var newTransaction = new Transaction
            {
                TransactionType = TransactionType.Deposit.ToString(),
                Amount = depositAmount,
                TransactionDate = DateTime.Now,
                CheckingAccountId = checkingAccount.Id,
                CustomerId = customerId
            };

            _dbContext.Transactions.Add(newTransaction);
            _dbContext.SaveChanges();

            return (DepositResult.Success, checkingAccount.Balance);
        }

        public (DepositResult Result, decimal NewBalance) DepositToSaving(Guid customerId, decimal depositAmount)
        {
            if (depositAmount <= 0)
                return (DepositResult.InvalidAmount, 0);
            
            var savingAccount = GetActiveSavingAccount(customerId);

            if (savingAccount == null)
                return (DepositResult.NoActiveAccount, 0);

            
            savingAccount.Balance += depositAmount;

            var newTransaction = new Transaction
            {
                TransactionType = TransactionType.Deposit.ToString(),
                Amount = depositAmount,
                TransactionDate = DateTime.Now,
                SavingAccountId = savingAccount.Id,
                CustomerId = customerId
            };

            _dbContext.Transactions.Add(newTransaction);
            _dbContext.SaveChanges();

            return (DepositResult.Success, savingAccount.Balance);
        }
        
        public (TransferResult Result, decimal CheckingBalance, decimal SavingBalance) TransferBetweenAccounts(Guid customerId, decimal transferAmount, bool fromCheckingToSaving)
        {
            if (transferAmount <= 0)
                return (TransferResult.InvalidAmount, 0, 0);

            var checkingAccount = GetActiveCheckingAccount(customerId);
            var savingAccount = GetActiveSavingAccount(customerId);

            if (checkingAccount == null)
                return (TransferResult.NoActiveAccount, 0, 0);
            if (savingAccount == null)
                return (TransferResult.NoActiveAccount, 0, 0);

            if (fromCheckingToSaving)
            {
                if (checkingAccount.Balance < transferAmount)
                    return (TransferResult.InsufficientFunds, checkingAccount.Balance, savingAccount.Balance);

                checkingAccount.Balance -= transferAmount;
                savingAccount.Balance += transferAmount;
            }
            else
            {
                if (savingAccount.Balance < transferAmount)
                    return (TransferResult.InsufficientFunds, checkingAccount.Balance, savingAccount.Balance);

                savingAccount.Balance -= transferAmount;
                checkingAccount.Balance += transferAmount;
            }

            var outgoingTransaction = new Transaction
            {
                TransactionType = TransactionType.Transfer.ToString(),
                Amount = -transferAmount,
                TransactionDate = DateTime.Now,
                CheckingAccountId = fromCheckingToSaving ? checkingAccount.Id : (Guid?)null,
                SavingAccountId = fromCheckingToSaving ? (Guid?)null : savingAccount.Id,
                CustomerId = customerId
            };

            var incomingTransaction = new Transaction
            {
                TransactionType = TransactionType.Transfer.ToString(),
                Amount = transferAmount,
                TransactionDate = DateTime.Now,
                CheckingAccountId = fromCheckingToSaving ? (Guid?)null : checkingAccount.Id,
                SavingAccountId = fromCheckingToSaving ? savingAccount.Id : (Guid?)null,
                CustomerId = customerId
            };

            _dbContext.Transactions.Add(outgoingTransaction);
            _dbContext.Transactions.Add(incomingTransaction);
            _dbContext.SaveChanges();

            return (TransferResult.Success, checkingAccount.Balance, savingAccount.Balance);
        }

        public (TransferResult Result, decimal CheckingBalance) TransferToRecipient(Guid customerId, decimal transferAmount, string phoneNumber)
        {
            if (transferAmount <= 0)
                return (TransferResult.InvalidAmount, 0);

            var customerCheckingAccount = GetActiveCheckingAccount(customerId);
            if (customerCheckingAccount == null)
                return (TransferResult.NoActiveAccount, 0);

            if (customerCheckingAccount.Balance < transferAmount)
                return (TransferResult.InsufficientFunds, customerCheckingAccount.Balance);

            var recipient = _dbContext.Customers.FirstOrDefault(c => c.PhoneNumber == phoneNumber);
            if (recipient == null)
                return (TransferResult.NoRecipientAccountFound, customerCheckingAccount.Balance);

            var recipientCheckingAccount = GetActiveCheckingAccount(recipient.Id);
            if (recipientCheckingAccount == null)
                return (TransferResult.NoActiveRecipientCheckingAccount, customerCheckingAccount.Balance);

            customerCheckingAccount.Balance -= transferAmount;
            recipientCheckingAccount.Balance += transferAmount;

            var outgoingTransaction = new Transaction
            {
                TransactionType = TransactionType.Transfer.ToString(),
                Amount = -transferAmount,
                TransactionDate = DateTime.Now,
                CheckingAccountId = customerCheckingAccount.Id,
                CustomerId = customerId
            };

            var incomingTransaction = new Transaction
            {
                TransactionType = TransactionType.Transfer.ToString(),
                Amount = +transferAmount,
                TransactionDate = DateTime.Now,
                CheckingAccountId = recipientCheckingAccount.Id,
                CustomerId = recipient.Id
            };

            _dbContext.Transactions.Add(outgoingTransaction);
            _dbContext.Transactions.Add(incomingTransaction);
            _dbContext.SaveChanges();

            return (TransferResult.Success, customerCheckingAccount.Balance);
        }

        public List<Transaction> GetLastTransactions(Guid customerId, int count = 5)
        {
            return _dbContext.Transactions
                .Where(t => t.CustomerId == customerId)
                .OrderByDescending(t => t.TransactionDate)
                .Take(count)
                .ToList();
        }

        public RequestChequeBookResult RequestChequeBook(Guid customerId)
        {
            var checkingAccount = GetActiveCheckingAccount(customerId);

            if (checkingAccount == null)
                return RequestChequeBookResult.NoActiveCheckingAccount;

            bool hasPendingRequest = _dbContext.ChequeBookRequests.Any(r =>
                r.CustomerId == customerId && r.Status == "Pending");

            if (hasPendingRequest)
                return RequestChequeBookResult.AlreadyPending;

            var newRequest = new ChequeBookRequest
            {
                RequestDate = DateTime.Now,
                Status = "Pending",
                CheckingAccountId = checkingAccount.Id,
                CustomerId = customerId
            };

            _dbContext.ChequeBookRequests.Add(newRequest);
            _dbContext.SaveChanges();

            return RequestChequeBookResult.Success;
        }

        public ChangePasswordResult ChangePassword(Guid customerId, string currentPassword, string newPassword)
        {
            var customer = _dbContext.Customers.FirstOrDefault(c => c.Id == customerId);

            if (customer == null || !BCrypt.Net.BCrypt.Verify(currentPassword, customer.Password))
                return ChangePasswordResult.IncorrectCurrentPassword;

            if (string.IsNullOrWhiteSpace(newPassword))
                return ChangePasswordResult.InvalidNewPassword;

            customer.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _dbContext.SaveChanges();

            return ChangePasswordResult.Success;
        }

    }
}