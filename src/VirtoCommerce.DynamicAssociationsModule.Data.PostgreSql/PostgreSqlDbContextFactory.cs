using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.DynamicAssociationsModule.Data.Repositories;

namespace VirtoCommerce.DynamicAssociationsModule.Data.PostgreSql
{
    public class PostgreSqlDbContextFactory : IDesignTimeDbContextFactory<AssociationsModuleDbContext>
    {
        public AssociationsModuleDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AssociationsModuleDbContext>();
            var connectionString = args.Any() ? args[0] : "User ID = postgres; Password = password; Host = localhost; Port = 5432; Database = virtocommerce3;";

            builder.UseNpgsql(
                connectionString,
                db => db.MigrationsAssembly(typeof(PostgreSqlDbContextFactory).Assembly.GetName().Name));

            return new AssociationsModuleDbContext(builder.Options);
        }
    }
}
