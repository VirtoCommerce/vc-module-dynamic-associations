using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.DynamicAssociationsModule.Data.Repositories;

namespace VirtoCommerce.DynamicAssociationsModule.Data.SqlServer
{
    public class SqlServerDbContextFactory : IDesignTimeDbContextFactory<AssociationsModuleDbContext>
    {
        public AssociationsModuleDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AssociationsModuleDbContext>();
            var connectionString = args.Any() ? args[0] : "Data Source=(local);Initial Catalog=VirtoCommerce3;Persist Security Info=True;User ID=virto;Password=virto;MultipleActiveResultSets=True;Connect Timeout=30";

            builder.UseSqlServer(
                connectionString,
                db => db.MigrationsAssembly(typeof(SqlServerDbContextFactory).Assembly.GetName().Name));

            return new AssociationsModuleDbContext(builder.Options);
        }
    }
}
