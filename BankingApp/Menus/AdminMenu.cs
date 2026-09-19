using BankingApp.Utils;
using BankingApp.Data.Services;
using BankingApp.Data.Models;
using System.Diagnostics;

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
                        int deleteCredentialAttempts = 0;
                        Customer? customerToDelete = null;

                        while (deleteCredentialAttempts < 3)
                        {
                            Console.Write("Enter customer's username: ");
                            string? deleteUsername = Console.ReadLine();

                            Console.Write("Enter customer's password: ");
                            string? deletePassword = Console.ReadLine();

                            var (found, passwordValid, customer) = customerService.VerifyCredentials(deleteUsername!, deletePassword!);

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
                                customerToDelete = customer;
                                break;
                            }

                            deleteCredentialAttempts++;
                        }

                        if (customerToDelete == null)
                        {
                            Console.WriteLine("Too many failed attempts. Returning to admin menu.");
                            break;
                        }

                        var (hasChecking, hasSaving) = adminService.GetActiveAccountsStatus(customerToDelete.Id);

                        if (!hasChecking && !hasSaving)
                        {
                            Console.WriteLine("This customer has no active accounts to delete.");
                            break;
                        }

                        Console.WriteLine("Which account(s) would you like to close?");

                        var validOptions = new List<int>();

                        if (hasSaving)
                        {
                            Console.WriteLine("1. Saving Account");
                            Console.WriteLine("2. Both Checking and Saving");
                            validOptions.Add(1);
                            validOptions.Add(2);
                        }
                        else
                        {
                            Console.WriteLine("1. Checking Account");
                            validOptions.Add(1);
                        }

                        Console.Write("Write a number to choose one of the options: ");

                        var (validDeleteSelection, deleteSelection) = InputHelper.GetMenuSelection(validOptions.ToArray());

                        if (!validDeleteSelection)
                        {
                            Console.WriteLine("Too many invalid attempts. Returning to admin menu.");
                            break;
                        }

                        if (hasChecking && hasSaving)
                        {
                            if (deleteSelection == 1)
                            {
                                adminService.CloseSavingAccount(customerToDelete.Id);
                                Console.WriteLine("Saving account closed.");
                            }
                            else
                            {
                                adminService.CloseCheckingAccount(customerToDelete.Id);
                                adminService.CloseSavingAccount(customerToDelete.Id);
                                Console.WriteLine("Checking and saving accounts closed.");
                            }
                        }
                        else if (hasChecking)
                        {
                            adminService.CloseCheckingAccount(customerToDelete.Id);
                            Console.WriteLine("Checking account closed.");
                        }
                        else
                        {
                            adminService.CloseSavingAccount(customerToDelete.Id);
                            Console.WriteLine("Saving account closed.");
                        }

                        break;
                    case 3:
                        int editCredentialAttempts = 0;
                        Customer? customerToEdit = null;

                        while (editCredentialAttempts < 3)
                        {
                            Console.Write("Enter customer's username: ");
                            string? editUsername = Console.ReadLine();

                            Console.Write("Enter customer's password: ");
                            string? editPassword = Console.ReadLine();

                            var (found, passwordValid, customer) = customerService.VerifyCredentials(editUsername!, editPassword!);

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
                                customerToEdit = customer;
                                break;
                            }

                            editCredentialAttempts++;
                        }

                        if (customerToEdit == null)
                        {
                            Console.WriteLine("Too many failed attempts. Returning to admin menu.");
                            break;
                        }

                        Console.WriteLine("What would you like to update?");
                        Console.WriteLine("1. Name");
                        Console.WriteLine("2. Phone Number");
                        Console.WriteLine("3. Email");
                        Console.Write("Write a number to choose one of the options: ");

                        var (validFieldSelection, fieldSelection) = InputHelper.GetMenuSelection(new[] { 1, 2, 3 });

                        if (!validFieldSelection)
                        {
                            Console.WriteLine("Too many invalid attempts. Returning to admin menu.");
                            break;
                        }

                        UpdateCustomerResult editResult = UpdateCustomerResult.InvalidInput;
                        int editAttempts = 0;

                        while (editAttempts < 3)
                        {
                            if (fieldSelection == 1)
                            {
                                Console.Write("Enter updated name: ");
                                string? updatedName = Console.ReadLine();
                                editResult = customerService.UpdateCustomerName(customerToEdit.Id, updatedName!);
                            }
                            else if (fieldSelection == 2)
                            {
                                Console.Write("Enter updated phone number: ");
                                string? updatedPhone = Console.ReadLine();
                                editResult = customerService.UpdateCustomerPhone(customerToEdit.Id, updatedPhone!);
                            }
                            else
                            {
                                Console.Write("Enter updated email: ");
                                string? updatedEmail = Console.ReadLine();
                                editResult = customerService.UpdateCustomerEmail(customerToEdit.Id, updatedEmail!);
                            }

                            if (editResult == UpdateCustomerResult.Success)
                                break;

                            Console.WriteLine(editResult switch
                            {
                                UpdateCustomerResult.InvalidInput => "Invalid input. Please try again.",
                                UpdateCustomerResult.DuplicateEmail => "That email is already in use. Please try again.",
                                UpdateCustomerResult.DuplicatePhoneNumber => "That phone number is already in use. Please try again.",
                                UpdateCustomerResult.CustomerNotFound => "Customer not found.",
                                _ => "Unknown error."
                            });

                            editAttempts++;
                        }

                        if (editResult == UpdateCustomerResult.Success)
                        {
                            Console.WriteLine("Customer info updated successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Too many invalid attempts. Returning to admin menu.");
                        }

                        break;
                    case 4:
                        adminService.DisplaySummary();
                        break;
                    case 5:
                        int passResetAttempts = 0;
                        Customer? custToUpdatePass = null;

                        while (passResetAttempts < 3)
                        {
                            Console.Write("Enter customer's username: ");
                            string? getCustUsername = Console.ReadLine();

                            var (verified, foundCustomer) = customerService.VerifyUsername(getCustUsername!);

                            if (verified)
                            {
                                Console.WriteLine("Customer Found!");
                                custToUpdatePass = foundCustomer;
                                break;
                            }
                            else
                            {
                                Console.WriteLine("No Customer Found");
                            }

                            passResetAttempts++;
                        }

                        if (custToUpdatePass == null)
                        {
                            Console.WriteLine("Too many failed attempts. Returning to admin menu.");
                            break;
                        }

                        int newPasswordAttempts = 0;
                        string? updatedCustPass = null;

                        while (newPasswordAttempts < 3)
                        {
                            Console.Write("Enter customer's updated password: ");
                            string? enteredPassword = Console.ReadLine();

                            if (string.IsNullOrWhiteSpace(enteredPassword))
                            {
                                Console.WriteLine("Password cannot be empty. Please try again.");
                                newPasswordAttempts++;
                                continue;
                            }

                            updatedCustPass = enteredPassword;
                            break;
                        }

                        if (updatedCustPass == null)
                        {
                            Console.WriteLine("Too many invalid attempts. Returning to admin menu.");
                            break;
                        }

                        customerService.ChangeCustomerPass(custToUpdatePass, updatedCustPass);
                        Console.WriteLine("Customer's password updated successfully");
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