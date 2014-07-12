using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace NGM.Forum {
    public class Permissions : IPermissionProvider {

        
        public static readonly Permission ManageForums = new Permission { Description = "Manage forums for others", Name = "ManageForums" };
        public static readonly Permission ManageOwnForums = new Permission { Description = "Manage own forums", Name = "ManageOwnForums", ImpliedBy = new[] { ManageForums } };

        public static readonly Permission MoveThread = new Permission { Description = "Move any thread to another forum", Name = "MoveThread" };
        public static readonly Permission MoveOwnThread = new Permission { Description = "Move your own thread to another forum", Name = "MoveOwnThread", ImpliedBy = new[] { MoveThread }};

        public static readonly Permission StickyThread = new Permission { Description = "Allows you to mark any thread as Sticky", Name = "StickyThread" };
        public static readonly Permission StickyOwnThread = new Permission { Description = "Allows you to mark your own thread as Sticky", Name = "StickyOwnThread", ImpliedBy = new[] { StickyThread } };

        public static readonly Permission CloseThread = new Permission { Description = "Allows you to close any thread", Name = "CloseThread" };
        public static readonly Permission CloseOwnThread = new Permission { Description = "Allows you to close your own thread", Name = "CloseOwnThread", ImpliedBy = new[] { CloseThread }};

        public static readonly Permission CanPostToForums = new Permission { ImpliedBy = new[] { ManageForums, ManageOwnForums }, Name = "PostToForums", Description = "Gives user permission to post in forums" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {

                ManageForums,
                ManageOwnForums,
                
                MoveOwnThread,
                MoveThread,
                StickyOwnThread,
                StickyThread,

                CloseOwnThread,
                CloseThread,

                CanPostToForums
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {

            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageForums}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {ManageForums}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] {ManageForums}
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {ManageOwnForums}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                },

                /*Need to handle*/
                new PermissionStereotype {
                    Name = "Anonymous",
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                },

            };
        }
    }
}