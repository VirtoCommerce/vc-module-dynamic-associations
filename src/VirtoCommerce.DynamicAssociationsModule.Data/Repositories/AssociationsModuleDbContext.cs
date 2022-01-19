using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.DynamicAssociationsModule.Data.Model;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Repositories
{
    public class AssociationsModuleDbContext : DbContextWithTriggers
    {
        public AssociationsModuleDbContext(DbContextOptions<AssociationsModuleDbContext> options)
            : base(options)
        {
        }

        protected AssociationsModuleDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AssociationEntity>().ToTable("DynamicAssociation").HasKey(x => x.Id);
            modelBuilder.Entity<AssociationEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<AssociationEntity>().HasIndex(x => new { x.StoreId, x.AssociationType }).IsUnique(false).HasDatabaseName("IX_StoreId_AssociationType");

            base.OnModelCreating(modelBuilder);
        }
    }
}
