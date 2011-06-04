using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace NGM.Forum {
    public class Permissions {
        public static readonly Permission ManageForums = new Permission { Description = "Manage forums", Name = "ManageForums" };

        public static readonly Permission AddThread = new Permission { Description = "Add a thread", Name = "AddThread" };
        public static readonly Permission EditThread = new Permission { Description = "Edit any thread", Name = "EditThread", ImpliedBy = new[] { AddThread } };
        public static readonly Permission DeleteThread = new Permission { Description = "Delete any thread", Name = "DeleteThread", ImpliedBy = new [] { AddThread }};

        public static readonly Permission AddPost = new Permission { Description = "Add a post", Name = "AddPost" };
        public static readonly Permission EditPost = new Permission { Description = "Edit a post", Name = "EditPost", ImpliedBy = new[] { AddPost } };
        public static readonly Permission DeletePost = new Permission { Description = "Delete a post", Name = "DeletePost", ImpliedBy = new[] { AddPost } };

        public static readonly Permission MetaListForum = new Permission { ImpliedBy = new[] { AddThread, EditThread, DeleteThread } };
        public static readonly Permission MetaListThread = new Permission { ImpliedBy = new[] { AddPost, EditPost, DeletePost } };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageForums,
                AddThread,
                EditThread, 
                DeleteThread,
                AddPost,
                EditPost,
                DeletePost
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageForums, MetaListForum, MetaListThread}
                },
                new PermissionStereotype {
                    Name = "Anonymous",
                    Permissions = new[] {MetaListForum, MetaListThread}
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] {MetaListForum, MetaListThread}
                },
                new PermissionStereotype {
                    Name = "Editor",
                    Permissions = new[] {MetaListForum, MetaListThread}
                },
                new PermissionStereotype {
                    Name = "Moderator",
                    Permissions = new[] {MetaListForum, MetaListThread}
                },
                new PermissionStereotype {
                    Name = "Author",
                    Permissions = new[] {MetaListForum, MetaListThread}
                },
                new PermissionStereotype {
                    Name = "Contributor",
                    Permissions = new[] {MetaListForum, MetaListThread}
                },
            };
        }

    }
}