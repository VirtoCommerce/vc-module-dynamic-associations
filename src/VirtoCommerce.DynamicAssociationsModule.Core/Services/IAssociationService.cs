using System.Threading.Tasks;
using VirtoCommerce.DynamicAssociationsModule.Core.Model;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Services
{
    public interface IAssociationService
    {
        Task<Association[]> GetByIdsAsync(string[] itemIds);
        Task SaveChangesAsync(Association[] items);
        Task DeleteAsync(string[] itemIds);
    }
}
