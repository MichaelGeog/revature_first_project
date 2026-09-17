bool exit = false;

while (!exit)
{
    System.Console.WriteLine("\n=======================");
    System.Console.WriteLine("| Welcome to the bank |");
    System.Console.WriteLine("=======================");
    System.Console.WriteLine("1. Customer");
    System.Console.WriteLine("2. Admin");
    System.Console.WriteLine("3. Exit");
    System.Console.Write("Write a number to choose one of the options: ");
    int userSelection = int.Parse(Console.ReadLine());
    switch (userSelection)
    {
        case 1:
            break;
        case 2:
            break;
        case 3:
            exit = true;
            break;
    }
}