using BankingApp.Utils;
using BankingApp.Data.Services;
using BankingApp.Data.Models;

namespace BankingApp.Menus
{
    public static class AdminMenu
    {
        public static void Show(ProjectBankingAppContext dbContext)
        {
            var adminService = new AdminService(dbContext);
            var customerService = new CustomerService(dbContext);

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
                        Console.WriteLine("1. New Customer");
                        Console.WriteLine("2. Existing Customer");
                        Console.Write("Write a number to choose one of the options: ");

                        var (validCustomerType, customerTypeSelection) = InputHelper.GetMenuSelection(new[] { 1, 2 });

                        if (!validCustomerType)
                        {
                            Console.WriteLine("Too many invalid attempts. Returning to admin menu.");
                            break;
                        }

                        if (customerTypeSelection == 1)
                        {
                            Console.WriteLine("1. Checking Account Only");
                            Console.WriteLine("2. Checking + Saving Account");
                            Console.Write("Write a number to choose one of the options: ");

                            var (validAccountType, accountTypeSelection) = InputHelper.GetMenuSelection(new[] { 1, 2 });

                            if (!validAccountType)
                            {
                                Console.WriteLine("Too many invalid attempts. Returning to admin menu.");
                                break;
                            }

                            adminService.CreateNewCustomerFlow(customerService, adminService, includeSaving: accountTypeSelection == 2);
                        }
                        else
                        {
                            int credentialAttempts = 0;
                            Customer? foundCustomer = null;

                            while (credentialAttempts < 3)
                            {
                                Console.Write("Enter customer's username: ");
                                string? existingUsername = Console.ReadLine();

                                Console.Write("Enter customer's password: ");
                                string? existingPassword = Console.ReadLine();

                                var (found, passwordValid, customer) = customerService.VerifyCredentials(existingUsername!, existingPassword!);

                                if (!found)
                                {
                                    Console.WriteLine("No customer found with that username.");
                                }
                                else if (!passwordValid)
                                {
                                    Console.WriteLine("Incorrect password.");
                                }
                                else
                                {
                                    foundCustomer = customer;
                                    break;
                                }

                                credentialAttempts++;
                            }

                            if (foundCustomer == null)
                            {
                                Console.WriteLine("Too many failed attempts. Returning to admin menu.");
                                break;
                            }

                            var savingStatus = adminService.CheckSavingAccountStatus(foundCustomer.Id);

                            if (savingStatus == ExistingCustomerAccountStatus.HasActiveSaving)
                            {
                                Console.WriteLine("This customer already has an active checking and saving account. Cannot create a new account.");
                            }
                            else
                            {
                                Console.WriteLine($"Creating a saving account for {foundCustomer.Name}.");

                                Console.Write("Enter initial balance for the saving account: ");
                                decimal.TryParse(Console.ReadLine(), out decimal savingBalance);

                                var savingAccount = adminService.CreateSavingAccount(foundCustomer.Id, savingBalance);

                                Console.WriteLine($"Saving account opened. Account #: {savingAccount.AccountNumber}, Balance: {savingAccount.Balance:C}");
                            }
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
    }
}