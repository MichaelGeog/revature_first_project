namespace BankingApp.Utils
{
    public static class Messages
    {
        public static void InvalidChoice()
        {
            Console.WriteLine("\n-----------------------------------------------------------------------");
            Console.WriteLine("****** Invalid number, Please enter a valid number from the menu ******");
            Console.WriteLine("-----------------------------------------------------------------------");
        }

        public static void CustomerGoodbye()
        {
            Console.WriteLine("\n==================================");
            Console.WriteLine(" Goodbye Customer! Logging Out...");
            Console.WriteLine("==================================");
        }

        public static void AdminGoodbye()
        {
            Console.WriteLine("\n===============================");
            Console.WriteLine(" Goodbye Admin! Logging Out...");
            Console.WriteLine("===============================");
        }

        public static void ProgramGoodbye()
        {
            Console.WriteLine("\n=============================");
            Console.WriteLine(" Goodbye! Exiting program...");
            Console.WriteLine("=============================");
        }
    }
}
