using VirtoCommerce.Platform.Security.Authorization;

namespace VirtoCommerce.DynamicAssociationsModule.Web.Authorization
{
    public class DynamicAssociationAuthorizationRequirement : PermissionAuthorizationRequirement
    {
        public DynamicAssociationAuthorizationRequirement(string permission)
            : base(permission)
        {
        }
    }
}
