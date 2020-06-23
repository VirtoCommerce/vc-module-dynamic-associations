using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.DynamicAssociationsModule.Data.Model;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Repositories
{
    public class DynamicAssociationsModuleDbContext : DbContextWithTriggers
    {
        public DynamicAssociationsModuleDbContext(DbContextOptions<DynamicAssociationsModuleDbContext> options)
            : base(options)
        {
        }

        protected DynamicAssociationsModuleDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DynamicAssociationEntity>().ToTable("DynamicAssociation").HasKey(x => x.Id);
            modelBuilder.Entity<DynamicAssociationEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<DynamicAssociationEntity>().HasIndex(x => new { x.StoreId, x.AssociationType }).IsUnique(false).HasName("IX_StoreId_AssociationType");

            base.OnModelCreating(modelBuilder);
        }
    }
}
