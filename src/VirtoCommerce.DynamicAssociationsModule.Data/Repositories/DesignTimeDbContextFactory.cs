using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using VirtoCommerce.CatalogModule.Data.Repositories;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Repositories
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DynamicAssociationsModuleDbContext>
    {
        public DynamicAssociationsModuleDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<DynamicAssociationsModuleDbContext>();

            builder.UseSqlServer("Data Source=(local);Initial Catalog=VirtoCommerce3;Persist Security Info=True;User ID=virto;Password=virto;MultipleActiveResultSets=True;Connect Timeout=30");

            return new DynamicAssociationsModuleDbContext(builder.Options);
        }
    }
}
