using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.CatalogModule.Data.Repositories
{
    public interface IDynamicAssociationsRepository : IRepository
    {
        IQueryable<DynamicAssociationEntity> DynamicAssociations { get; }
        Task<DynamicAssociationEntity[]> GetDynamicAssociationsByIdsAsync(string[] dynamicAssociationIds);
    }
}
