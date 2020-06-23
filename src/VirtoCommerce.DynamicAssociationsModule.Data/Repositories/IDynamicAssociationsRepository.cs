using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.DynamicAssociationsModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Repositories
{
    public interface IDynamicAssociationsRepository : IRepository
    {
        IQueryable<DynamicAssociationEntity> DynamicAssociations { get; }
        Task<DynamicAssociationEntity[]> GetDynamicAssociationsByIdsAsync(string[] dynamicAssociationIds);
    }
}
