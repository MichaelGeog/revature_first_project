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
    }
}