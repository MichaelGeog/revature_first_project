namespace BankingApp.Utils
{
    public static class InputHelper
    {
        public static (bool Success, int Selection) GetMenuSelection(int[] validOptions, int maxAttempts = 3)
        {
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                if (int.TryParse(Console.ReadLine(), out int selection) && validOptions.Contains(selection))
                    return (true, selection);

                Messages.InvalidChoice();
                attempts++;
            }

            return (false, 0);
        }
    
        public static (bool Success, decimal Amount) GetPositiveDecimal(string prompt, int maxAttempts = 3)
        {
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                Console.Write(prompt);

                if (decimal.TryParse(Console.ReadLine(), out decimal amount) && amount > 0)
                    return (true, amount);

                Console.WriteLine("\n**** Invalid amount. Please enter a positive number. ****");
                attempts++;
            }

            return (false, 0);
        }

        public static void WaitForEscToContinue()
        {
            Console.WriteLine("\nPress ESC to return to the menu...");

            while (Console.ReadKey(intercept: true).Key != ConsoleKey.Escape)
            {
            }

            Console.Clear();
        }
    
        public static string ReadPassword()
        {
            var password = new System.Text.StringBuilder();
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
            while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password.ToString();
        }
    }
}