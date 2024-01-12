using System.Reflection;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.DynamicAssociationsModule.Data.Model;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Repositories
{
    public class AssociationsModuleDbContext : DbContextBase
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

            // Allows configuration for an entity type for different database types.
            // Applies configuration from all <see cref="IEntityTypeConfiguration{TEntity}" in VirtoCommerce.DynamicAssociationsModule.Data.XXX project. /> 
            switch (this.Database.ProviderName)
            {
                case "Pomelo.EntityFrameworkCore.MySql":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.DynamicAssociationsModule.Data.MySql"));
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.DynamicAssociationsModule.Data.PostgreSql"));
                    break;
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.DynamicAssociationsModule.Data.SqlServer"));
                    break;
            }

        }
    }
}
