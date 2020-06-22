using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Repositories
{
	public class VirtoCommerceDynamicAssociationsModuleDbContext : DbContextWithTriggers
	{
		public VirtoCommerceDynamicAssociationsModuleDbContext(DbContextOptions<VirtoCommerceDynamicAssociationsModuleDbContext> options)
		  : base(options)
		{
		}

		protected VirtoCommerceDynamicAssociationsModuleDbContext(DbContextOptions options)
			: base(options)
		{
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			//        modelBuilder.Entity<MyModuleEntity>().ToTable("MyModule").HasKey(x => x.Id);
			//        modelBuilder.Entity<MyModuleEntity>().Property(x => x.Id).HasMaxLength(128);
			//        base.OnModelCreating(modelBuilder);
		}
	}
}

