using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NGM.Forum.Models;
using Orchard.ContentManagement;

namespace NGM.Forum.Helpers {
    public static class HierarchyHelpers {
        public static ForumPart GetForum(IContent content) {
            var postPart = content.As<PostPart>();
            var threadPart = content.As<ThreadPart>();

            if (postPart == null) {
                return threadPart == null ? null : threadPart.ForumPart;
            }
            return postPart.ThreadPart.ForumPart;
        }

        public static ThreadPart GetThreadPart(IContent content) {
            return content.Has<ThreadPart>() ? content.As<ThreadPart>() : content.As<PostPart>().ThreadPart;
        }
    }
}