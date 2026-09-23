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

    }
}