using BankingApp.Menus;
using BankingApp.Utils;

using var dbContext = DbContextFactory.Create();
WelcomeMenu.Show(dbContext);
