using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.DynamicAssociationsModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DynamicAssociationsModule.Data.Repositories
{
    public interface IAssociationsRepository : IRepository
    {
        IQueryable<AssociationEntity> Associations { get; }
        Task<AssociationEntity[]> GetAssociationsByIdsAsync(string[] associationIds);
    }
}
