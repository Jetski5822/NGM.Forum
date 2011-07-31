using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace NGM.Forum {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageForums = new Permission { Description = "Manage forums", Name = "ManageForums" };
        public static readonly Permission ViewForum = new Permission { Description = "View forum", Name = "ViewForum" };

        public static readonly Permission CreatePost = new Permission { Description = "Create a post", Name = "CreatePost" };
        public static readonly Permission ReplyPost = new Permission { Description = "Reply to a post", Name = "ReplyPost" };
        public static readonly Permission ViewPost = new Permission { Description = "View post", Name = "ViewPost" };

        public static readonly Permission DeleteOwnPost = new Permission { Description = "Delete your own post", Name = "DeleteOwnPost" };
        public static readonly Permission DeleteAnyPost = new Permission { Description = "Delete any post", Name = "DeleteAnyPost" };

        public static readonly Permission EditOwnPost = new Permission { Description = "Edit your own post", Name = "EditOwnPost" };
        public static readonly Permission EditAnyPost = new Permission { Description = "Edit any post", Name = "EditAnyPost" };

        public static readonly Permission MoveThread = new Permission { Description = "Move thread to another forum", Name = "MoveThread" };

        //public static readonly Permission MoveAnyPost = new Permission { Description = "Move any post", Name = "MoveAnyPost" };

        public static readonly Permission ManageOpenCloseThread = new Permission { Description = "Can open/close a thread", Name = "ManageOpenCloseThread" };
        public static readonly Permission ManageStickyThread = new Permission { Description = "Can create a thread that is sticky", Name = "ManageStickyThread" };

        public static readonly Permission MarkPostInOwnThreadAsAnswer = new Permission { Description = "Can mark your a post in your own thread as an answer", Name = "MarkPostInOwnThreadAsAnswer" };
        public static readonly Permission MarkAnyPostAsAnswer = new Permission { Description = "Can mark any post as an answer", Name = "MarkAnyPostAsAnswer" };
      
        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageForums,
                CreatePost,
                ReplyPost, 
                DeleteOwnPost,
                DeleteAnyPost,
                EditOwnPost,
                EditAnyPost,
                MoveThread,
                //MoveAnyPost,
                ManageOpenCloseThread,
                ManageStickyThread,
                MarkPostInOwnThreadAsAnswer,
                MarkAnyPostAsAnswer
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {

            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageForums}
                },
                new PermissionStereotype {
                    Name = "Anonymous",
                    Permissions = new Permission[] {},
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new Permission[] {},
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new Permission[] {},
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new Permission[] {},
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new Permission[] {},
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new Permission[] {},
                },
            };
        }
    }
}