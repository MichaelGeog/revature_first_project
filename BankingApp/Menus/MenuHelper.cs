using BankingApp.Utils;

namespace BankingApp.Menus
{
    public static class MenuHelper
    {
        public static void ShowWelcomeMenu()
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
                        ShowAdminMenu();
                        break;
                    case 3:
                        Messages.ProgramGoodbye();
                        exit = true;
                        break;
                }
            }
        }

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

        public static void ShowAdminMenu()
        {
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