using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Repositories
{
	public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<VirtoCommerceDynamicAssociationsModuleDbContext>
	{
		public VirtoCommerceDynamicAssociationsModuleDbContext CreateDbContext(string[] args)
		{
			var builder = new DbContextOptionsBuilder<VirtoCommerceDynamicAssociationsModuleDbContext>();

			builder.UseSqlServer("Data Source=(local);Initial Catalog=VirtoCommerce3;Persist Security Info=True;User ID=virto;Password=virto;MultipleActiveResultSets=True;Connect Timeout=30");

			return new VirtoCommerceDynamicAssociationsModuleDbContext(builder.Options);
		}
	}
}
