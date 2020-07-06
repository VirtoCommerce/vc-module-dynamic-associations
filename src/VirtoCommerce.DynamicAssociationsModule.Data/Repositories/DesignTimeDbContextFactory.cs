using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Repositories
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AssociationsModuleDbContext>
    {
        public AssociationsModuleDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<AssociationsModuleDbContext>();

            builder.UseSqlServer("Data Source=(local);Initial Catalog=VirtoCommerce3;Persist Security Info=True;User ID=virto;Password=virto;MultipleActiveResultSets=True;Connect Timeout=30");

            return new AssociationsModuleDbContext(builder.Options);
        }
    }
}
