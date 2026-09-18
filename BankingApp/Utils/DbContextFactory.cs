using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using BankingApp.Data.Models;

namespace BankingApp.Utils
{
    public static class DbContextFactory
    {
        public static ProjectBankingAppContext Create()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<ProjectBankingAppContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ProjectBankingAppContext(optionsBuilder.Options);
        }
    }
}