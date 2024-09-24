using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.MarketplaceCommissionsModule.Data.Repositories;

namespace VirtoCommerce.MarketplaceCommissionsModule.Data.PostgreSql
{
    public class PostgreSqlDbContextFactory : IDesignTimeDbContextFactory<CommissionFeeDbContext>
    {
        public CommissionFeeDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CommissionFeeDbContext>();
            var connectionString = args.Any() ? args[0] : "User ID = postgres; Password = password; Host = localhost; Port = 5432; Database = virtocommerce3;";

            builder.UseNpgsql(
                connectionString,
                db => db.MigrationsAssembly(typeof(PostgreSqlDbContextFactory).Assembly.GetName().Name));

            return new CommissionFeeDbContext(builder.Options);
        }
    }
}
