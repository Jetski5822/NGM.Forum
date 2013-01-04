using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Security;
using Orchard.Security.Permissions;

namespace NGM.Forum.Security {
    [UsedImplicitly]
    public class ForumAuthorizationEventHandler : IAuthorizationServiceEventHandler {
        public void Checking(CheckAccessContext context) { }
        public void Complete(CheckAccessContext context) { }

        public void Adjust(CheckAccessContext context) {
            if (!context.Granted &&
                context.Content.Is<ICommonPart>()) {
                if (OwnerVariationExists(context.Permission) &&
                    HasOwnership(context.User, context.Content)) {
                    context.Adjusted = true;
                    context.Permission = GetOwnerVariation(context.Permission);
                }
            }
        }

        private static bool HasOwnership(IUser user, IContent content) {
            if (user == null || content == null)
                return false;

            var common = content.As<ICommonPart>();
            if (common == null || common.Owner == null)
                return false;

            return user.Id == common.Owner.Id;
        }

        private static bool OwnerVariationExists(Permission permission) {
            return GetOwnerVariation(permission) != null;
        }

        private static Permission GetOwnerVariation(Permission permission) {
            if (permission.Name == Permissions.ManageForums.Name)
                return Permissions.ManageOwnForums;
            if (permission.Name == Permissions.DeletePost.Name)
                return Permissions.DeleteOwnPost;
            if (permission.Name == Permissions.EditPost.Name)
                return Permissions.EditOwnPost;
            if (permission.Name == Permissions.MoveThread.Name)
                return Permissions.MoveOwnThread;
            if (permission.Name == Permissions.StickyThread.Name)
                return Permissions.StickyOwnThread;
            if (permission.Name == Permissions.CloseThread.Name)
                return Permissions.CloseOwnThread;
            return null;
        }
    }
}