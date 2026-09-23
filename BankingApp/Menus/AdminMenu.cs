using BankingApp.Utils;
using BankingApp.Data.Services;
using BankingApp.Data.Models;
using System.Diagnostics;

namespace BankingApp.Menus
{
    public static class AdminMenu
    {
        private static void CreateNewCustomerFlow(CustomerService customerService, AdminService adminService, bool includeSaving)
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

            var (result, customer) = customerService.CreateCustomer(newUsername, newPassword, newName, newPhoneNumber, newEmail);

            if (result != CreateCustomerResult.Success)
            {
                Console.WriteLine(result switch
                {
                    CreateCustomerResult.InvalidInput => "Invalid input. Please check all fields.",
                    CreateCustomerResult.DuplicateUsername => "That username is already taken.",
                    CreateCustomerResult.DuplicateEmail => "That email is already in use.",
                    CreateCustomerResult.DuplicatePhoneNumber => "That phone number is already in use.",
                    _ => "Unknown error."
                });
                return;
            }

            var (validCheckingBalance, checkingBalance) = InputHelper.GetPositiveDecimal("Enter initial balance for the checking account: ");

            if (!validCheckingBalance)
            {
                Console.WriteLine("Too many invalid attempts. Returning to admin menu.");
                return;
            }

            var checkingAccount = adminService.CreateCheckingAccount(customer!.Id, checkingBalance);

            Console.WriteLine($"Customer '{customer.Name}' created successfully.");
            Console.WriteLine($"Checking account opened. Account #: {checkingAccount.AccountNumber}, Balance: {checkingAccount.Balance:C}");

            if (includeSaving)
            {
                var (validSavingBalance, savingBalance) = InputHelper.GetPositiveDecimal("Enter initial balance for the saving account: ");

                if (!validSavingBalance)
                {
                    Console.WriteLine("Too many invalid attempts. Returning to admin menu.");
                    return;
                }

                var savingAccount = adminService.CreateSavingAccount(customer.Id, savingBalance);

                Console.WriteLine($"Saving account opened. Account #: {savingAccount.AccountNumber}, Balance: {savingAccount.Balance:C}");
            }
        }

        public static void Show(ProjectBankingAppContext dbContext)
        {
            var adminService = new AdminService(dbContext);
            var customerService = new CustomerService(dbContext);

            int attempts = 0;
            Admin? loggedInAdmin = null;

            while (attempts < 3)
            {
                Console.Write("\nEnter Your Username: \n");
                string? adminUsername = Console.ReadLine();
                Console.Write("Enter Your Password: \n");
                string? adminPassword = Console.ReadLine();

                var (found, passwordValid, admin) = adminService.VerifyCredentials(adminUsername!, adminPassword!);

                if (!found)
                {
                    Console.WriteLine("\n**** Admin not found! ****");
                }
                else if (!passwordValid)
                {
                    Console.WriteLine("\n**** Invalid Credentials: Please try again. ****");
                }
                else
                {
                    Console.WriteLine("\nCredentials Verified");
                    loggedInAdmin = admin;
                    break;
                }

                attempts++;
            }

            if (loggedInAdmin == null)
            {
                Console.WriteLine("\n**** Too many failed attempts. Returning to main menu. ****");
                return;
            }

            bool exit3 = false;
            while (!exit3)
            {
                Console.WriteLine("\n------------------------------------------");
                Console.WriteLine($"Welcome {loggedInAdmin.Name}!, How can we help you today?");
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
                        Console.WriteLine("\n1. New Customer");
                        Console.WriteLine("2. Existing Customer");
                        Console.Write("Write a number to choose one of the options: ");

                        var (validCustomerType, customerTypeSelection) = InputHelper.GetMenuSelection(new[] { 1, 2 });

                        if (!validCustomerType)
                        {
                            Console.WriteLine("\n**** Too many invalid attempts. Returning to admin menu. ****");
                            break;
                        }

                        if (customerTypeSelection == 1)
                        {
                            Console.WriteLine("\n1. Checking Account Only");
                            Console.WriteLine("2. Checking + Saving Account");
                            Console.Write("Write a number to choose one of the options: ");

                            var (validAccountType, accountTypeSelection) = InputHelper.GetMenuSelection(new[] { 1, 2 });

                            if (!validAccountType)
                            {
                                Console.WriteLine("\n**** Too many invalid attempts. Returning to admin menu. ****");
                                break;
                            }

                            CreateNewCustomerFlow(customerService, adminService, includeSaving: accountTypeSelection == 2);
                        }
                        else
                        {
                            int credentialAttempts = 0;
                            Customer? foundCustomer = null;

                            while (credentialAttempts < 3)
                            {
                                Console.Write("\nEnter customer's username: ");
                                string? existingUsername = Console.ReadLine();

                                Console.Write("Enter customer's password: ");
                                string? existingPassword = Console.ReadLine();

                                var (found, passwordValid, customer) = customerService.VerifyCredentials(existingUsername!, existingPassword!);

                                if (!found)
                                {
                                    Console.WriteLine("\n**** No customer found with that username. ****");
                                }
                                else if (!passwordValid)
                                {
                                    Console.WriteLine("\n**** Incorrect password. ****");
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
                                Console.WriteLine("\n**** Too many failed attempts. Returning to admin menu. ****");
                                break;
                            }

                            var savingStatus = adminService.CheckSavingAccountStatus(foundCustomer.Id);

                            if (savingStatus == ExistingCustomerAccountStatus.HasActiveSaving)
                            {
                                Console.WriteLine("\n**** This customer already has an active checking and saving account. Cannot create a new account. ****");
                            }
                            else
                            {
                                Console.WriteLine($"\nCreating a saving account for {foundCustomer.Name}.");

                                var (validSavingBalance, savingBalance) = InputHelper.GetPositiveDecimal("Enter initial balance for the saving account: ");

                                if (!validSavingBalance)
                                {
                                    Console.WriteLine("\n**** Too many invalid attempts. Returning to admin menu. ****");
                                    break;
                                }

                                var savingAccount = adminService.CreateSavingAccount(foundCustomer.Id, savingBalance);

                                Console.WriteLine($"\nSaving account opened. Account #: {savingAccount.AccountNumber}, Balance: {savingAccount.Balance:C}");
                            }
                        }
                        break;
                    case 2:
                        int deleteCredentialAttempts = 0;
                        Customer? customerToDelete = null;

                        while (deleteCredentialAttempts < 3)
                        {
                            Console.Write("\nEnter customer's username: ");
                            string? deleteUsername = Console.ReadLine();

                            Console.Write("Enter customer's password: ");
                            string? deletePassword = Console.ReadLine();

                            var (found, passwordValid, customer) = customerService.VerifyCredentials(deleteUsername!, deletePassword!);

                            if (!found)
                            {
                                Console.WriteLine("\n**** No customer found with that username. ****");
                            }
                            else if (!passwordValid)
                            {
                                Console.WriteLine("\n**** Incorrect password. ****");
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
                            Console.WriteLine("\n**** Too many failed attempts. Returning to admin menu. ****");
                            break;
                        }

                        var (hasChecking, hasSaving) = adminService.GetActiveAccountsStatus(customerToDelete.Id);

                        if (!hasChecking && !hasSaving)
                        {
                            Console.WriteLine("\n**** This customer has no active accounts to delete. ****");
                            break;
                        }

                        Console.WriteLine("\nWhich account(s) would you like to close?");

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

                        Console.Write("\nWrite a number to choose one of the options: ");

                        var (validDeleteSelection, deleteSelection) = InputHelper.GetMenuSelection(validOptions.ToArray());

                        if (!validDeleteSelection)
                        {
                            Console.WriteLine("\n**** Too many invalid attempts. Returning to admin menu. ****");
                            break;
                        }

                        if (hasChecking && hasSaving)
                        {
                            if (deleteSelection == 1)
                            {
                                adminService.CloseSavingAccount(customerToDelete.Id);
                                Console.WriteLine("\n**** Saving account closed. ****");
                            }
                            else
                            {
                                adminService.CloseCheckingAccount(customerToDelete.Id);
                                adminService.CloseSavingAccount(customerToDelete.Id);
                                Console.WriteLine("\n**** Checking and saving accounts closed. ****");
                            }
                        }
                        else if (hasChecking)
                        {
                            adminService.CloseCheckingAccount(customerToDelete.Id);
                            Console.WriteLine("\n**** Checking account closed. ****");
                        }
                        else
                        {
                            adminService.CloseSavingAccount(customerToDelete.Id);
                            Console.WriteLine("\n**** Saving account closed. ****");
                        }

                        break;
                    case 3:
                        int editCredentialAttempts = 0;
                        Customer? customerToEdit = null;

                        while (editCredentialAttempts < 3)
                        {
                            Console.Write("\nEnter customer's username: ");
                            string? editUsername = Console.ReadLine();

                            Console.Write("Enter customer's password: ");
                            string? editPassword = Console.ReadLine();

                            var (found, passwordValid, customer) = customerService.VerifyCredentials(editUsername!, editPassword!);

                            if (!found)
                            {
                                Console.WriteLine("\n**** No customer found with that username. ****");
                            }
                            else if (!passwordValid)
                            {
                                Console.WriteLine("\n**** Incorrect password. ****");
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
                            Console.WriteLine("\n**** Too many failed attempts. Returning to admin menu. ****");
                            break;
                        }

                        Console.WriteLine("\nWhat would you like to update?");
                        Console.WriteLine("1. Name");
                        Console.WriteLine("2. Phone Number");
                        Console.WriteLine("3. Email");
                        Console.Write("Write a number to choose one of the options: ");

                        var (validFieldSelection, fieldSelection) = InputHelper.GetMenuSelection(new[] { 1, 2, 3 });

                        if (!validFieldSelection)
                        {
                            Console.WriteLine("\n**** Too many invalid attempts. Returning to admin menu. ****");
                            break;
                        }

                        UpdateCustomerResult editResult = UpdateCustomerResult.InvalidInput;
                        int editAttempts = 0;

                        while (editAttempts < 3)
                        {
                            if (fieldSelection == 1)
                            {
                                Console.Write("\nEnter updated name: ");
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
                                UpdateCustomerResult.InvalidInput => "\n**** Invalid input. Please try again. ****",
                                UpdateCustomerResult.DuplicateEmail => "\n**** That email is already in use. Please try again. ****",
                                UpdateCustomerResult.DuplicatePhoneNumber => "\n**** That phone number is already in use. Please try again. ****",
                                UpdateCustomerResult.CustomerNotFound => "\n**** Customer not found. ****",
                                _ => "\n**** Unknown error. ****"
                            });

                            editAttempts++;
                        }

                        if (editResult == UpdateCustomerResult.Success)
                        {
                            Console.WriteLine("\nCustomer info updated successfully.");
                        }
                        else
                        {
                            Console.WriteLine("\n**** Too many invalid attempts. Returning to admin menu. ****");
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
                            Console.Write("\nEnter customer's username: ");
                            string? getCustUsername = Console.ReadLine();

                            var (verified, foundCustomer) = customerService.VerifyUsername(getCustUsername!);

                            if (verified)
                            {
                                Console.WriteLine("\nCustomer Found!");
                                custToUpdatePass = foundCustomer;
                                break;
                            }
                            else
                            {
                                Console.WriteLine("\n**** No Customer Found ****");
                            }

                            passResetAttempts++;
                        }

                        if (custToUpdatePass == null)
                        {
                            Console.WriteLine("\n**** Too many failed attempts. Returning to admin menu. ****");
                            break;
                        }

                        int newPasswordAttempts = 0;
                        string? updatedCustPass = null;

                        while (newPasswordAttempts < 3)
                        {
                            Console.Write("\nEnter customer's updated password: ");
                            string? enteredPassword = Console.ReadLine();

                            if (string.IsNullOrWhiteSpace(enteredPassword))
                            {
                                Console.WriteLine("\n**** Password cannot be empty. Please try again. ****");
                                newPasswordAttempts++;
                                continue;
                            }

                            updatedCustPass = enteredPassword;
                            break;
                        }

                        if (updatedCustPass == null)
                        {
                            Console.WriteLine("\n**** Too many invalid attempts. Returning to admin menu. ****");
                            break;
                        }

                        customerService.ChangeCustomerPass(custToUpdatePass, updatedCustPass);
                        Console.WriteLine("\nCustomer's password updated successfully");
                        break;
                    case 6:
                        var pendingRequests = adminService.GetPendingChequeBookRequests();

                        if (pendingRequests.Count == 0)
                        {
                            Console.WriteLine("No pending cheque book requests.");
                            break;
                        }

                        Console.WriteLine("\n===== Pending Cheque Book Requests =====");
                        for (int i = 0; i < pendingRequests.Count; i++)
                        {
                            var request = pendingRequests[i];
                            Console.WriteLine($"{i + 1}. {request.Customer.Name} - Account #: {request.CheckingAccount.AccountNumber} - Requested: {request.RequestDate}");
                        }
                        Console.WriteLine("=========================================");
                        Console.Write("Select a request number: ");

                        var requestOptions = Enumerable.Range(1, pendingRequests.Count).ToArray();
                        var (validRequestSelection, requestSelection) = InputHelper.GetMenuSelection(requestOptions);

                        if (!validRequestSelection)
                        {
                            Console.WriteLine("Too many invalid attempts. Returning to admin menu.");
                            break;
                        }

                        var chosenRequest = pendingRequests[requestSelection - 1];

                        Console.WriteLine($"\n1. Approve\n2. Deny");
                        Console.Write("Write a number to choose one of the options: ");

                        var (validDecision, decision) = InputHelper.GetMenuSelection(new[] { 1, 2 });

                        if (!validDecision)
                        {
                            Console.WriteLine("Too many invalid attempts. Returning to admin menu.");
                            break;
                        }

                        if (decision == 1)
                        {
                            adminService.ApproveChequeBookRequest(chosenRequest.Id, loggedInAdmin.Id);
                            Console.WriteLine("Request approved.");
                        }
                        else
                        {
                            adminService.DenyChequeBookRequest(chosenRequest.Id, loggedInAdmin.Id);
                            Console.WriteLine("Request denied.");
                        }

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