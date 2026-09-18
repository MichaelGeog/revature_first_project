using BankingApp.Utils;
using BankingApp.Data.Services;
using BankingApp.Data.Models;
using System.Net;

namespace BankingApp.Menus
{
    public static class MenuHelper
    {
        #region Welcome/Main Menu
        public static void ShowWelcomeMenu(ProjectBankingAppContext dbContext)
        {
            bool exit = false;

            while (!exit)
            {
                Console.WriteLine("\n=======================");
                Console.WriteLine("| Welcome to the bank |");
                Console.WriteLine("=======================");
                Console.WriteLine("1. Customer");
                Console.WriteLine("2. Admin");
                Console.WriteLine("3. Exit");
                Console.Write("Write a number to choose one of the options: ");

                if (!int.TryParse(Console.ReadLine(), out int userSelection) || userSelection is not (1 or 2 or 3))
                {
                    Messages.InvalidChoice();
                    continue;
                }

                switch (userSelection)
                {
                    case 1:
                        ShowCustomerMenu();
                        break;
                    case 2:
                        ShowAdminMenu(dbContext);
                        break;
                    case 3:
                        Messages.ProgramGoodbye();
                        exit = true;
                        break;
                }
            }
        }
        #endregion

        #region Customer Menu Method
        public static void ShowCustomerMenu()
        {
            bool exit2 = false;

            while (!exit2)
            {
                Console.WriteLine("\n---------------------------------------------");
                Console.WriteLine("Welcome customer!, How can we help you today?");
                Console.WriteLine("---------------------------------------------");
                Console.WriteLine("1. Check Account Details");
                Console.WriteLine("2. Withdraw");
                Console.WriteLine("3. Deposit");
                Console.WriteLine("4. Transfer");
                Console.WriteLine("5. Last 5 transactions");
                Console.WriteLine("6. Request Cheque Book");
                Console.WriteLine("7. Change Password");
                Console.WriteLine("8. Exit");
                Console.Write("Write a number to choose one of the options: ");

                if (!int.TryParse(Console.ReadLine(), out int userSelection2) || userSelection2 is not (1 or 2 or 3 or 4 or 5 or 6 or 7 or 8))
                {
                    Messages.InvalidChoice();
                    continue;
                }

                switch (userSelection2)
                {
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                    case 7:
                        break;
                    case 8:
                        Messages.CustomerGoodbye();
                        exit2 = true;
                        break;
                }
            }
        }
        #endregion

        #region Admin Menu Method
        public static void ShowAdminMenu(ProjectBankingAppContext dbContext)
        {
            var adminService = new AdminService(dbContext);

            int attempts = 0;
            bool loginSuccessful = false;

            while (attempts < 3)
            {
                Console.Write("\nEnter Your Username: \n");
                string? adminUsername = Console.ReadLine();
                Console.Write("Enter Your Password: \n");
                string? adminPassword = Console.ReadLine();

                bool? loginResult = adminService.Login(adminUsername, adminPassword);

                if (loginResult == true)
                {
                    Console.WriteLine("\nCredentials Verified: Welcome");
                    loginSuccessful = true;
                    break;
                }
                else if (loginResult == false)
                {
                    Console.WriteLine("\nInvalid Credentials: Please try again.");
                }
                else
                {
                    Console.WriteLine("\nAdmin not found!");
                }

                attempts++;
            }

            if (!loginSuccessful)
            {
                Console.WriteLine("Too many failed attempts. Returning to main menu.");
                return;
            }

            bool exit3 = false;
            while (!exit3)
            {
                Console.WriteLine("\n------------------------------------------");
                Console.WriteLine("Welcome admin!, How can we help you today?");
                Console.WriteLine("------------------------------------------");
                Console.WriteLine("1. Create New Account");
                Console.WriteLine("2. Delete Account");
                Console.WriteLine("3. Edit Account Details");
                Console.WriteLine("4. Display Summary");
                Console.WriteLine("5. Reset Customer Password");
                Console.WriteLine("6. Approve Cheque book request");
                Console.WriteLine("7. Exit");
                Console.Write("Write a number to choose one of the options: ");

                if (!int.TryParse(Console.ReadLine(), out int userSelection3) || userSelection3 is not (1 or 2 or 3 or 4 or 5 or 6 or 7))
                {
                    Messages.InvalidChoice();
                    continue;
                }

                switch (userSelection3)
                {
                    case 1:
                        int customerTypeAttempts = 0;
                        int customerTypeSelection = 0;
                        bool validCustomerType = false;

                        while (customerTypeAttempts < 3)
                        {
                            Console.WriteLine("1. New Customer");
                            Console.WriteLine("2. Existing Customer");
                            Console.Write("Write a number to choose one of the options: ");

                            if (!int.TryParse(Console.ReadLine(), out customerTypeSelection) || customerTypeSelection is not (1 or 2))
                            {
                                Messages.InvalidChoice();
                                customerTypeAttempts++;
                                continue;
                            }

                            validCustomerType = true;
                            break;
                        }

                        if (!validCustomerType)
                        {
                            Console.WriteLine("Too many invalid attempts. Returning to admin menu.");
                            break; // breaks out of the switch, back to the admin menu loop
                        }

                        if (customerTypeSelection == 1)
                        {
                            int accountTypeAttempts = 0;
                            int accountTypeSelection = 0;
                            bool validAccountType = false;

                            while (accountTypeAttempts < 3)
                            {
                                Console.WriteLine("1. Checking Account Only");
                                Console.WriteLine("2. Checking + Saving Account");
                                Console.Write("Write a number to choose one of the options: ");

                                if (!int.TryParse(Console.ReadLine(), out accountTypeSelection) || accountTypeSelection is not (1 or 2))
                                {
                                    Messages.InvalidChoice();
                                    accountTypeAttempts++;
                                    continue;
                                }

                                validAccountType = true;
                                break;
                            }

                            if (!validAccountType)
                            {
                                Console.WriteLine("Too many invalid attempts. Returning to admin menu.");
                                break;
                            }

                            if (accountTypeSelection == 1)
                            {
                                Console.Write("Create a username: ");
                                string? newUsername = Console.ReadLine();

                                Console.Write("Create a password: ");
                                string? newPassword = Console.ReadLine();

                                Console.Write("Enter customer's full name: ");
                                string? newName = Console.ReadLine();

                                Console.Write("Enter customer's phone number: ");
                                string? newPhoneNumber = Console.ReadLine();

                                Console.Write("Enter customer's email: ");
                                string? newEmail = Console.ReadLine();

                                var (result, customer) = adminService.CreateCustomer(newUsername, newPassword, newName, newPhoneNumber, newEmail);

                                switch (result)
                                {
                                    case CreateCustomerResult.Success:
                                        Console.Write("Enter initial balance for the checking account: ");
                                        decimal.TryParse(Console.ReadLine(), out decimal initialBalance);

                                        var account = adminService.CreateCheckingAccount(customer!.Id, initialBalance);

                                        Console.WriteLine($"Customer '{customer.Name}' created successfully.");
                                        Console.WriteLine($"Checking account opened. Account #: {account.AccountNumber}, Routing #: {account.RoutingNumber}, Balance: {account.Balance:C}");
                                        break;
                                    case CreateCustomerResult.InvalidInput:
                                        Console.WriteLine("Invalid input. Please check all fields.");
                                        break;
                                    case CreateCustomerResult.DuplicateUsername:
                                        Console.WriteLine("That username is already taken.");
                                        break;
                                    case CreateCustomerResult.DuplicateEmail:
                                        Console.WriteLine("That email is already in use.");
                                        break;
                                    case CreateCustomerResult.DuplicatePhoneNumber:
                                        Console.WriteLine("That phone number is already in use.");
                                        break;
                                }
                            }
                            else
                            {
                                // create customer + checking account + saving account
                            }
                        }
                        else
                        {
                            // Existing customer flow
                        }
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    case 5:
                        break;
                    case 6:
                        break;
                    case 7:
                        Messages.AdminGoodbye();
                        exit3 = true;
                        break;
                }
            }
        }
        #endregion
    }
}