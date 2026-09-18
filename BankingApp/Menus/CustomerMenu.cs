using BankingApp.Utils;

namespace BankingApp.Menus
{
    public static class CustomerMenu
    {
        public static void Show()
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
    }
}