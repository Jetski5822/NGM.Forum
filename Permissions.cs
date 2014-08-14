using System.Collections.Generic;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace NGM.Forum {
    public class Permissions : IPermissionProvider {
        //Actions on thread/posts,etc at that atomic level are identified as follows:
        // -- POST --
        // DELETE, CREATE, EDIT(Own/Other), MOVE(Own/Other) , MAKE-STICKY

        // -- THREAD --
        // DELETE, CREATE, MOVE(Own/Other) , Close( Own/Other)

        // ADMIN inapropriate post reports

        // --  FORUM/CATEGORY/FORUMS-HOME  
        //     -- these are basically admin functions and as such can be grouped together
        //     DELETE, CREATE/EDIT

        //  TYPICAL USER ROLES: Admin, Moderator, general user
        // ADMIN can : do anything in the forums (but maybe not edit others posts/threads)
        // Moderater can:  manage inappropriate posts, Move/Delete/Close threads and Move, Delete, Posts (edit others possibly)
        // General user: make threads, posts, edit posts and possibly close own threads.

      
        public static readonly Permission ManageForums = new Permission { Description = "Manage forums for others: add forums to existing categories, move and close threads and posts", Name = "ManageForums"};
        public static readonly Permission ManageOwnForums = new Permission { Description = "Manage own forums: add forums to existing categories, move and close threads and posts", Name = "ManageOwnForums", ImpliedBy = new[] { ManageForums } };

        public static readonly Permission MoveThread = new Permission { Description = "Move any thread to another forum", Name = "MoveThread", ImpliedBy = new[] { ManageForums } };
        public static readonly Permission MoveOwnThread = new Permission { Description = "Move own thread to another forum", Name = "MoveOwnThread", ImpliedBy = new[] { MoveThread }};

        public static readonly Permission StickyThread = new Permission { Description = "Mark thread as Sticky", Name = "StickyThread", ImpliedBy = new[] { ManageForums } };
        public static readonly Permission StickyOwnThread = new Permission { Description = "Mark own Thread as Sticky", Name = "StickyOwnThread", ImpliedBy = new[] { StickyThread } };

        public static readonly Permission CloseThread = new Permission { Description = "Close threads.", Name = "CloseThread", ImpliedBy = new[] { ManageForums }};
        public static readonly Permission CloseOwnThread = new Permission { Description = "Close own threads.", Name = "CloseOwnThread", ImpliedBy = new[] {CloseThread } };

        //TODO would be nice to have a way to allow users to create thread and posts only in specific forums (home pages)
        //There is no 'create own threads  and posts' because it does not seem useful for the majority of use cases.
        //It would only allow a user to post to their own forum.. more like a blog

        public static readonly Permission CreateThreadsAndPosts = new Permission { ImpliedBy = new[] { ManageForums }, Name = "CreateThreadsAndPosts", Description = "Create threads and posts." };

        public static readonly Permission DeleteThreadsAndPosts = new Permission { Description = "Delete threads and posts", Name = "DeleteThreadsAndPosts", ImpliedBy = new[] { ManageForums } };
        public static readonly Permission DeleteOwnThreadsAndPosts = new Permission { Description = "Delete own threads and posts", Name = "DeleteOwnThreadsAndPosts", ImpliedBy = new[] { DeleteThreadsAndPosts } };

        public static readonly Permission EditPosts = new Permission { ImpliedBy = new[] { ManageForums }, Name = "EditPosts", Description = "Allows editing of posts made by any user." };
        public static readonly Permission EditOwnPosts = new Permission { ImpliedBy = new[] { EditPosts }, Name = "EditOwnPosts", Description = "Allows a user to edit their own posts." };

        public static readonly Permission ModerateInappropriatePosts = new Permission { Description = "Moderate inappropriate post reports in all forums.", Name = "ModerateInappropriatePosts", ImpliedBy = new[] { ManageForums } };
        public static readonly Permission ModerateOwnInappropriatePosts = new Permission { Description = "Moderate inappropriate post reports in own forums.", Name = "ModerateOwnInappropriatePosts", ImpliedBy = new[] { ModerateInappropriatePosts } };


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

                CreateThreadsAndPosts,

                DeleteThreadsAndPosts,
                DeleteOwnThreadsAndPosts,

                EditPosts,
                EditOwnPosts,

                ModerateInappropriatePosts,
                ModerateOwnInappropriatePosts

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