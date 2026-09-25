using BankingApp.Utils;
using BankingApp.Data.Services;
using BankingApp.Data.Models;
using System.Text.RegularExpressions;


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

            Console.Clear();

            while (attempts < 3)
            {
                Console.Write("\nEnter Your Username: \n");
                string? custUsername = Console.ReadLine();
                Console.Write("Enter Your Password: \n");
                string custPassword = InputHelper.ReadPassword();

                var (found, passwordValid, customer) = customerService.VerifyCredentials(custUsername!, custPassword);

                if (!found)
                {
                    Console.WriteLine("\n**** No customer found with that username. ****");
                }
                else if (!passwordValid)
                {
                    Console.WriteLine("\n**** Invalid Credentials: Please try again. ****");
                }
                else
                {
                    Console.WriteLine("\nCredentials Verified");
                    loggedInCustomer = customer;
                    break;
                }

                attempts++;
            }

            if (loggedInCustomer == null)
            {
                Console.WriteLine("\n**** Too many failed attempts. Returning to main menu. ****");
                InputHelper.WaitForEscToContinue();
                return;
            }

            bool exit2 = false;

            while (!exit2)
            {
                Console.Clear();
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
                        Console.Clear();
                        var (hasChecking, hasSaving) = customerService.GetActiveAccountsStatus(loggedInCustomer.Id);

                        if (hasChecking && !hasSaving)
                        {
                            Console.Clear();
                            var checkingAccount = customerService.GetActiveCheckingAccount(loggedInCustomer.Id);
                            PrintCheckingAccount(checkingAccount!);
                        }
                        else if (hasChecking && hasSaving)
                        {
                            Console.WriteLine("\nWhich account would you like to view?");
                            Console.WriteLine("1. Checking Account");
                            Console.WriteLine("2. Saving Account");
                            Console.Write("Write a number to choose one of the options: ");

                            var (validSelection, accountSelection) = InputHelper.GetMenuSelection(new[] { 1, 2 });

                            if (!validSelection)
                            {
                                Console.WriteLine("\n**** Too many invalid attempts. Returning to customer menu. ****");
                                InputHelper.WaitForEscToContinue();
                                break;
                            }

                            if (accountSelection == 1)
                            {
                                Console.Clear();
                                var checkingAccount = customerService.GetActiveCheckingAccount(loggedInCustomer.Id);
                                PrintCheckingAccount(checkingAccount!);
                            }
                            else
                            {
                                Console.Clear();
                                var savingAccount = customerService.GetActiveSavingAccount(loggedInCustomer.Id);
                                PrintSavingAccount(savingAccount!);
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nNo active accounts found.");
                        }
                        InputHelper.WaitForEscToContinue();
                        break;
                    case 2:
                        Console.Clear();
                        int withdrawAttempts = 0;
                        decimal withdrawAmount = 0;
                        bool validWithdrawAmount = false;

                        while (withdrawAttempts < 3)
                        {
                            Console.Write("How much would you like to withdraw: ");

                            if (!decimal.TryParse(Console.ReadLine(), out withdrawAmount) || withdrawAmount <= 0)
                            {
                                Console.WriteLine("\n**** Invalid amount, please enter a valid positive number. ****");
                                withdrawAttempts++;
                                continue;
                            }

                            validWithdrawAmount = true;
                            break;
                        }

                        if (!validWithdrawAmount)
                        {
                            Console.WriteLine("\n**** Too many invalid attempts. Returning to customer menu. ****");
                            InputHelper.WaitForEscToContinue();
                            break;
                        }

                        var (withdrawResult, newBalance) = customerService.Withdraw(loggedInCustomer.Id, withdrawAmount);

                        Console.WriteLine(withdrawResult switch
                        {
                            WithdrawResult.Success => $"\nWithdrawal successful. Your new balance is: {newBalance:C}",
                            WithdrawResult.InsufficientFunds => $"\n**** Insufficient funds. Your current balance is: {newBalance:C} ****",
                            WithdrawResult.NoActiveAccount => "\n**** No active checking account found. ****",
                            WithdrawResult.InvalidAmount => "\n**** Invalid withdrawal amount. ****",
                            _ => "\n**** Unknown error. ****"
                        });
                        InputHelper.WaitForEscToContinue();
                        break;
                    case 3:
                        Console.Clear();
                        var (hasCheckingForDeposit, hasSavingForDeposit) = customerService.GetActiveAccountsStatus(loggedInCustomer.Id);

                        if (!hasCheckingForDeposit && !hasSavingForDeposit)
                        {
                            Console.WriteLine("\n**** No active accounts found. ****");
                            InputHelper.WaitForEscToContinue();
                            break;
                        }

                        int depositAccountSelection = 1; 

                        if (hasCheckingForDeposit && hasSavingForDeposit)
                        {
                            Console.WriteLine("\nWhich account would you like to deposit into?");
                            Console.WriteLine("1. Checking Account");
                            Console.WriteLine("2. Saving Account");
                            Console.Write("Write a number to choose one of the options: ");

                            var (validDepositSelection, selection) = InputHelper.GetMenuSelection(new[] { 1, 2 });

                            if (!validDepositSelection)
                            {
                                Console.WriteLine("\n**** Too many invalid attempts. Returning to customer menu. ****");
                                InputHelper.WaitForEscToContinue();
                                break;
                            }

                            depositAccountSelection = selection;
                        }

                        int depositAttempts = 0;
                        decimal depositAmount = 0;
                        bool validDepositAmount = false;

                        Console.Clear();
                        while (depositAttempts < 3)
                        {
                            Console.Write("\nHow much would you like to deposit: ");

                            if (!decimal.TryParse(Console.ReadLine(), out depositAmount) || depositAmount <= 0)
                            {
                                Console.WriteLine("\n**** Invalid amount, please enter a valid positive number. ****");
                                depositAttempts++;
                                continue;
                            }

                            validDepositAmount = true;
                            break;
                        }

                        if (!validDepositAmount)
                        {
                            Console.WriteLine("\n**** Too many invalid attempts. Returning to customer menu. ****");
                            InputHelper.WaitForEscToContinue();
                            break;
                        }

                        (DepositResult depositResult, decimal newDepositBalance) result;

                        if (depositAccountSelection == 1)
                            result = customerService.DepositToChecking(loggedInCustomer.Id, depositAmount);
                        else
                            result = customerService.DepositToSaving(loggedInCustomer.Id, depositAmount);

                        Console.WriteLine(result.depositResult switch
                        {
                            DepositResult.Success => $"\nDeposit successful. Your new balance is: {result.newDepositBalance:C}",
                            DepositResult.NoActiveAccount => "\n**** No active account found. ****",
                            DepositResult.InvalidAmount => "\n**** Invalid deposit amount. ****",
                            _ => "\n**** Unknown error. ****"
                        });
                        InputHelper.WaitForEscToContinue();
                        break;
                    case 4:
                        Console.Clear();
                        var (hasCheckingForTransfer, hasSavingForTransfer) = customerService.GetActiveAccountsStatus(loggedInCustomer.Id);

                        if (!hasCheckingForTransfer)
                        {
                            Console.WriteLine("\n**** No active checking account found. ****");
                            InputHelper.WaitForEscToContinue();
                            break;
                        }

                        bool transferToSomeone;

                        if (hasSavingForTransfer)
                        {
                            Console.WriteLine("\n1. Transfer Between My Accounts");
                            Console.WriteLine("2. Transfer To Someone");
                            Console.Write("Write a number to choose one of the options: ");

                            var (validTransferTypeSelection, transferTypeSelection) = InputHelper.GetMenuSelection(new[] { 1, 2 });

                            if (!validTransferTypeSelection)
                            {
                                Console.WriteLine("\n**** Too many invalid attempts. Returning to customer menu. ****");
                                InputHelper.WaitForEscToContinue();
                                break;
                            }

                            transferToSomeone = transferTypeSelection == 2;
                        }
                        else
                        {
                            transferToSomeone = true;
                        }

                        if (!transferToSomeone)
                        {
                            Console.Clear();
                            Console.WriteLine("\n1. Checking to Saving");
                            Console.WriteLine("2. Saving to Checking");
                            Console.Write("Write a number to choose one of the options: ");

                            var (validDirectionSelection, directionSelection) = InputHelper.GetMenuSelection(new[] { 1, 2 });

                            if (!validDirectionSelection)
                            {
                                Console.WriteLine("\n**** Too many invalid attempts. Returning to customer menu. ****");
                                InputHelper.WaitForEscToContinue();
                                break;
                            }

                            int betweenAttempts = 0;
                            decimal betweenAmount = 0;
                            bool validBetweenAmount = false;

                            Console.Clear();
                            while (betweenAttempts < 3)
                            {
                                Console.Write("\nHow much would you like to transfer: ");

                                if (!decimal.TryParse(Console.ReadLine(), out betweenAmount) || betweenAmount <= 0)
                                {
                                    Console.WriteLine("\n**** Invalid amount, please enter a valid positive number. ****");
                                    betweenAttempts++;
                                    continue;
                                }

                                validBetweenAmount = true;
                                break;
                            }

                            if (!validBetweenAmount)
                            {
                                Console.WriteLine("\n**** Too many invalid attempts. Returning to customer menu. ****");
                                InputHelper.WaitForEscToContinue();
                                break;
                            }

                            var betweenResult = customerService.TransferBetweenAccounts(loggedInCustomer.Id, betweenAmount, fromCheckingToSaving: directionSelection == 1);

                            Console.WriteLine(betweenResult.Result switch
                            {
                                TransferResult.Success => $"\nTransfer successful. Checking balance: {betweenResult.CheckingBalance:C}, Saving balance: {betweenResult.SavingBalance:C}",
                                TransferResult.InvalidAmount => "\n**** Invalid transfer amount. ****",
                                TransferResult.NoActiveAccount => "\n**** One or more accounts are not active. ****",
                                TransferResult.InsufficientFunds => "\n**** Insufficient funds for this transfer. ****",
                                _ => "\n**** Unknown error. ****"
                            });
                        }
                        else
                        {
                            int phoneAttempts = 0;
                            string? recipientPhone = null;
                            bool validRecipientPhone = false;
                            Console.Clear();
                            while (phoneAttempts < 3)
                            {
                                Console.Write("\nEnter recipient's phone number: ");
                                string? enteredPhone = Console.ReadLine();

                                string digitsOnlyPhone = enteredPhone == null ? "" : Regex.Replace(enteredPhone, @"[^\d]", "");

                                if (digitsOnlyPhone.Length != 10)
                                {
                                    Console.WriteLine("\n**** Invalid phone number. Please enter a valid 10-digit phone number. ****");
                                    phoneAttempts++;
                                    continue;
                                }

                                recipientPhone = digitsOnlyPhone;
                                validRecipientPhone = true;
                                break;
                            }

                            if (!validRecipientPhone)
                            {
                                Console.WriteLine("\n**** Too many invalid attempts. Returning to customer menu. ****");
                                InputHelper.WaitForEscToContinue();
                                break;
                            }

                            int someoneAttempts = 0;
                            decimal someoneAmount = 0;
                            bool validSomeoneAmount = false;
                            Console.Clear();
                            while (someoneAttempts < 3)
                            {
                                Console.Write("\nHow much would you like to transfer: ");

                                if (!decimal.TryParse(Console.ReadLine(), out someoneAmount) || someoneAmount <= 0)
                                {
                                    Console.WriteLine("\n**** Invalid amount, please enter a valid positive number. ****");
                                    someoneAttempts++;
                                    continue;
                                }

                                validSomeoneAmount = true;
                                break;
                            }

                            if (!validSomeoneAmount)
                            {
                                Console.WriteLine("\n**** Too many invalid attempts. Returning to customer menu. ****");
                                InputHelper.WaitForEscToContinue();
                                break;
                            }

                            var someoneResult = customerService.TransferToRecipient(loggedInCustomer.Id, someoneAmount, recipientPhone!);

                            Console.WriteLine(someoneResult.Result switch
                            {
                                TransferResult.Success => $"\nTransfer successful. Your new checking balance is: {someoneResult.CheckingBalance:C}",
                                TransferResult.InvalidAmount => "\n**** Invalid transfer amount. ****",
                                TransferResult.NoActiveAccount => "\n**** You have no active checking account. ****",
                                TransferResult.InsufficientFunds => "\n**** Insufficient funds for this transfer. ****",
                                TransferResult.NoRecipientAccountFound => "\n**** No customer found with that phone number. ****",
                                TransferResult.NoActiveRecipientCheckingAccount => "\n**** Recipient has no active checking account. ****",
                                _ => "\n**** Unknown error. ****"
                            });
                        }
                        InputHelper.WaitForEscToContinue();
                        break;
                    case 5:
                        Console.Clear();
                        var recentTransactions = customerService.GetLastTransactions(loggedInCustomer.Id);

                        if (recentTransactions.Count == 0)
                        {
                            Console.WriteLine("\nNo transactions found.");
                            InputHelper.WaitForEscToContinue();
                            break;
                        }

                        Console.WriteLine("\n===== Last 5 Transactions =====");

                        foreach (var transaction in recentTransactions)
                        {
                            string accountType = transaction.CheckingAccountId != null ? "Checking" : "Saving";
                            Console.WriteLine($"{transaction.TransactionDate}: {transaction.TransactionType} {transaction.Amount:C} ({accountType})");
                        }

                        Console.WriteLine("================================");
                        InputHelper.WaitForEscToContinue();
                        break;
                    case 6:
                        Console.Clear();
                        var chequeResult = customerService.RequestChequeBook(loggedInCustomer.Id);

                        Console.WriteLine(chequeResult switch
                        {
                            RequestChequeBookResult.Success => "\nCheque book request submitted successfully.",
                            RequestChequeBookResult.NoActiveCheckingAccount => "\n**** You need an active checking account to request a cheque book. ****",
                            RequestChequeBookResult.AlreadyPending => "\n**** You already have a pending cheque book request. ****",
                            _ => "\n**** Unknown error. ****"
                        });
                        InputHelper.WaitForEscToContinue();
                        break;
                    case 7:
                        Console.Clear();
                        int currentPassAttempts = 0;
                        string? verifiedCurrentPassword = null;

                        while (currentPassAttempts < 3)
                        {
                            Console.Write("\nEnter your current password: ");
                            string enteredCurrentPassword = InputHelper.ReadPassword();

                            bool isCorrect = BCrypt.Net.BCrypt.Verify(enteredCurrentPassword, loggedInCustomer.Password);

                            if (!isCorrect)
                            {
                                Console.WriteLine("\n**** Incorrect password. Please try again. ****");
                                currentPassAttempts++;
                                continue;
                            }

                            verifiedCurrentPassword = enteredCurrentPassword;
                            break;
                        }

                        if (verifiedCurrentPassword == null)
                        {
                            Console.WriteLine("\n**** Too many failed attempts. Returning to customer menu. ****");
                            InputHelper.WaitForEscToContinue();
                            break;
                        }

                        int newPassAttempts = 0;
                        string? newPassword = null;

                        while (newPassAttempts < 3)
                        {
                            Console.Write("\nEnter your new password: ");
                            string enteredNewPassword = InputHelper.ReadPassword();

                            if (string.IsNullOrWhiteSpace(enteredNewPassword))
                            {
                                Console.WriteLine("\n**** Password cannot be empty. Please try again. ****");
                                newPassAttempts++;
                                continue;
                            }

                            newPassword = enteredNewPassword;
                            break;
                        }

                        if (newPassword == null)
                        {
                            Console.WriteLine("\n**** Too many invalid attempts. Returning to customer menu. ****");
                            InputHelper.WaitForEscToContinue();
                            break;
                        }

                        var changeResult = customerService.ChangePassword(loggedInCustomer.Id, verifiedCurrentPassword, newPassword);

                        Console.WriteLine(changeResult switch
                        {
                            ChangePasswordResult.Success => "\nPassword changed successfully.",
                            ChangePasswordResult.IncorrectCurrentPassword => "\n**** Current password was incorrect. ****",
                            ChangePasswordResult.InvalidNewPassword => "\n**** New password was invalid. ****",
                            _ => "\n**** Unknown error. ****"
                        });
                        InputHelper.WaitForEscToContinue();
                        break;
                    case 8:
                        Messages.CustomerGoodbye();
                        InputHelper.WaitForEscToContinue();
                        exit2 = true;
                        break;
                }
            }
        }
    }
}