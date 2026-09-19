using BankingApp.Utils;
using BankingApp.Data.Services;
using BankingApp.Data.Models;

namespace BankingApp.Menus
{
    public static class CustomerMenu
    {
        private static void PrintCheckingAccount(CheckingAccount account)
        {
            Console.WriteLine("\nChecking Account:");
            Console.WriteLine($"Account Number: {account.AccountNumber}");
            Console.WriteLine($"Routing Number: {account.RoutingNumber}");
            Console.WriteLine($"Balance: {account.Balance:C}");
            Console.WriteLine($"Date Opened: {account.DateOpen}");
            Console.WriteLine($"Status: {account.Status}");
        }

        private static void PrintSavingAccount(SavingAccount account)
        {
            Console.WriteLine("\nSaving Account:");
            Console.WriteLine($"Account Number: {account.AccountNumber}");
            Console.WriteLine($"Routing Number: {account.RoutingNumber}");
            Console.WriteLine($"Balance: {account.Balance:C}");
            Console.WriteLine($"Interest Rate: {account.InterestRate * 100:0.00}%");
            Console.WriteLine($"Withdrawal Limit: {account.WithdrawalLimit:C}");
            Console.WriteLine($"Date Opened: {account.DateOpen}");
            Console.WriteLine($"Status: {account.Status}");
        }

        public static void Show(ProjectBankingAppContext dbContext)
        {
            var customerService = new CustomerService(dbContext);

            int attempts = 0;
            Customer? loggedInCustomer = null;

            while (attempts < 3) 
            {
                Console.Write("\nEnter Your Username: \n");
                string? custUsername = Console.ReadLine();
                Console.Write("Enter Your Password: \n");
                string? custPassword = Console.ReadLine();

                var (found, passwordValid, customer) = customerService.VerifyCredentials(custUsername!, custPassword!);

                if (!found)
                {
                    Console.WriteLine("\nNo customer found with that username.");
                }
                else if (!passwordValid)
                {
                    Console.WriteLine("\nInvalid Credentials: Please try again.");
                }
                else
                {
                    Console.WriteLine("\nCredentials Verified: Welcome");
                    loggedInCustomer = customer;
                    break;
                }

                attempts++;
            }

            if (loggedInCustomer == null)
            {
                Console.WriteLine("Too many failed attempts. Returning to main menu.");
                return;
            }

            bool exit2 = false;

            while (!exit2)
            {
                Console.WriteLine("\n---------------------------------------------");
                Console.WriteLine($"Welcome {loggedInCustomer.Name}!, How can we help you today?");
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
                        var (hasChecking, hasSaving) = customerService.GetActiveAccountsStatus(loggedInCustomer.Id);

                        if (hasChecking && !hasSaving)
                        {
                            var checkingAccount = customerService.GetActiveCheckingAccount(loggedInCustomer.Id);
                            PrintCheckingAccount(checkingAccount!);
                        }
                        else if (hasChecking && hasSaving)
                        {
                            Console.WriteLine("Which account would you like to view?");
                            Console.WriteLine("1. Checking Account");
                            Console.WriteLine("2. Saving Account");
                            Console.Write("Write a number to choose one of the options: ");

                            var (validSelection, accountSelection) = InputHelper.GetMenuSelection(new[] { 1, 2 });

                            if (!validSelection)
                            {
                                Console.WriteLine("Too many invalid attempts. Returning to customer menu.");
                                break;
                            }

                            if (accountSelection == 1)
                            {
                                var checkingAccount = customerService.GetActiveCheckingAccount(loggedInCustomer.Id);
                                PrintCheckingAccount(checkingAccount!);
                            }
                            else
                            {
                                var savingAccount = customerService.GetActiveSavingAccount(loggedInCustomer.Id);
                                PrintSavingAccount(savingAccount!);
                            }
                        }
                        else
                        {
                            Console.WriteLine("No active accounts found.");
                        }
                        break;
                    case 2:
                        int withdrawAttempts = 0;
                        decimal withdrawAmount = 0;
                        bool validWithdrawAmount = false;

                        while (withdrawAttempts < 3)
                        {
                            Console.Write("How much would you like to withdraw: ");

                            if (!decimal.TryParse(Console.ReadLine(), out withdrawAmount) || withdrawAmount <= 0)
                            {
                                Console.WriteLine("Invalid amount, please enter a valid positive number.");
                                withdrawAttempts++;
                                continue;
                            }

                            validWithdrawAmount = true;
                            break;
                        }

                        if (!validWithdrawAmount)
                        {
                            Console.WriteLine("Too many invalid attempts. Returning to customer menu.");
                            break;
                        }

                        var (withdrawResult, newBalance) = customerService.Withdraw(loggedInCustomer.Id, withdrawAmount);

                        Console.WriteLine(withdrawResult switch
                        {
                            WithdrawResult.Success => $"Withdrawal successful. Your new balance is: {newBalance:C}",
                            WithdrawResult.InsufficientFunds => $"Insufficient funds. Your current balance is: {newBalance:C}",
                            WithdrawResult.NoActiveAccount => "No active checking account found.",
                            WithdrawResult.InvalidAmount => "Invalid withdrawal amount.",
                            _ => "Unknown error."
                        });

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
    }
}