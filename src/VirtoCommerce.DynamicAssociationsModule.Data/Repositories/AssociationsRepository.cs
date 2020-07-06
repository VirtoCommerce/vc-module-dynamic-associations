using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.DynamicAssociationsModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Repositories
{
    public class AssociationsRepository : DbContextRepositoryBase<AssociationsModuleDbContext>, IAssociationsRepository
    {
        public AssociationsRepository(AssociationsModuleDbContext dbContext)
            : base(dbContext)
        {
        }

        public IQueryable<AssociationEntity> Associations => DbContext.Set<AssociationEntity>();

        public async Task<AssociationEntity[]> GetAssociationsByIdsAsync(string[] associationIds)
        {
            var result = Array.Empty<AssociationEntity>();

            if (!associationIds.IsNullOrEmpty())
            {
                result = await Associations
                    .Where(x => associationIds.Contains(x.Id))
                    .ToArrayAsync();
            }

            return result;
        }
    }
}
