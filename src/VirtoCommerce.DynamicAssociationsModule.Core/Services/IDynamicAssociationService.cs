using System.Threading.Tasks;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Services
{
    public interface IDynamicAssociationService
    {
        Task<DynamicAssociation[]> GetByIdsAsync(string[] itemIds);
        Task SaveChangesAsync(DynamicAssociation[] items);
        Task DeleteAsync(string[] itemIds);
    }
}
