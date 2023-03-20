using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.DynamicAssociationsModule.Data.Repositories;

namespace VirtoCommerce.DynamicAssociationsModule.Data.MySql
{
    public class MySqlDbContextFactory : IDesignTimeDbContextFactory<AssociationsModuleDbContext>
    {
        public AssociationsModuleDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AssociationsModuleDbContext>();
            var connectionString = args.Any() ? args[0] : "server=localhost;user=root;password=virto;database=VirtoCommerce3;";
            var serverVersion = args.Length >= 2 ? args[1] : null;

            builder.UseMySql(
                connectionString,
                ResolveServerVersion(serverVersion, connectionString),
                db => db
                    .MigrationsAssembly(typeof(MySqlDbContextFactory).Assembly.GetName().Name));

            return new AssociationsModuleDbContext(builder.Options);
        }

        private static ServerVersion ResolveServerVersion(string? serverVersion, string connectionString)
        {
            if (serverVersion == "AutoDetect")
            {
                return ServerVersion.AutoDetect(connectionString);
            }
            else if (serverVersion != null)
            {
                return ServerVersion.Parse(serverVersion);
            }
            return new MySqlServerVersion(new Version(5, 7));
        }
    }
}
