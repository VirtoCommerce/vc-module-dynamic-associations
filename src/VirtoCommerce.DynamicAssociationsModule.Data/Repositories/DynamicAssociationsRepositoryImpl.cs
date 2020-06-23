using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.DynamicAssociationsModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Repositories
{
    public class DynamicAssociationsRepositoryImpl : DbContextRepositoryBase<DynamicAssociationsModuleDbContext>, IDynamicAssociationsRepository
    {
        public DynamicAssociationsRepositoryImpl(DynamicAssociationsModuleDbContext dbContext)
            : base(dbContext)
        {
        }

        public IQueryable<DynamicAssociationEntity> DynamicAssociations => DbContext.Set<DynamicAssociationEntity>();

        public async Task<DynamicAssociationEntity[]> GetDynamicAssociationsByIdsAsync(string[] dynamicAssociationIds)
        {
            var result = Array.Empty<DynamicAssociationEntity>();

            if (!dynamicAssociationIds.IsNullOrEmpty())
            {
                result = await DynamicAssociations
                    .Where(x => dynamicAssociationIds.Contains(x.Id))
                    .ToArrayAsync();
            }

            return result;
        }
    }
}
