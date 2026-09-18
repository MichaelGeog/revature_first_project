using BankingApp.Utils;
using BankingApp.Data.Models;

namespace BankingApp.Menus
{
    public static class WelcomeMenu
    {
        public static void Show(ProjectBankingAppContext dbContext)
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
                        CustomerMenu.Show();
                        break;
                    case 2:
                        AdminMenu.Show(dbContext);
                        break;
                    case 3:
                        Messages.ProgramGoodbye();
                        exit = true;
                        break;
                }
            }
        }
    }
}