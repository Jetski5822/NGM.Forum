using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace NGM.Forum {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageForums = new Permission { Description = "Manage forums for others", Name = "ManageForums" };
        public static readonly Permission ManageOwnForums = new Permission { Description = "Manage own forums", Name = "ManageOwnForums", ImpliedBy = new[] { ManageForums } };

        public static readonly Permission ViewForum = new Permission { Description = "View forum", Name = "ViewForum" };

        public static readonly Permission CreatePost = new Permission { Description = "Create a post", Name = "CreatePost" };
        public static readonly Permission ReplyPost = new Permission { Description = "Reply to a post", Name = "ReplyPost" };
        public static readonly Permission ViewPost = new Permission { Description = "View post", Name = "ViewPost" };

        public static readonly Permission DeleteOwnPost = new Permission { Description = "Delete your own post", Name = "DeleteOwnPost" };
        public static readonly Permission DeletePost = new Permission { Description = "Delete any post", Name = "DeletePost" };

        public static readonly Permission EditOwnPost = new Permission { Description = "Edit your own post", Name = "EditOwnPost" };
        public static readonly Permission EditPost = new Permission { Description = "Edit any post", Name = "EditPost" };

        public static readonly Permission MoveThread = new Permission { Description = "Move thread to another forum", Name = "MoveThread" };

        //public static readonly Permission MoveAnyPost = new Permission { Description = "Move any post", Name = "MoveAnyPost" };
      
        public virtual Feature Feature { get; set; }

        public static readonly Permission MetaListForums = new Permission {};// ImpliedBy = new[] { EditBlogPost, PublishBlogPost, DeleteBlogPost } };

        public static readonly Permission MetaListOwnForums = new Permission {};// ImpliedBy = new[] { EditOwnBlogPost, PublishOwnBlogPost, DeleteOwnBlogPost } };

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageForums,
                ViewForum,
                
                CreatePost,
                ReplyPost, 
                ViewPost,

                DeleteOwnPost,
                DeletePost,
                
                EditOwnPost,
                EditPost,
                
                MoveThread,
                //MoveAnyPost,
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
                },
                new PermissionStereotype {
                    Name = "Moderator",
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
                    Permissions = new[] {ViewForum, ViewPost},
                },
                new PermissionStereotype {
                    Name = "Authenticated",
                    Permissions = new[] {ViewForum, ViewPost},
                },
            };
        }
    }
}