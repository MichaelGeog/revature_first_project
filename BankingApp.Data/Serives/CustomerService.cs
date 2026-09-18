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
    }
}