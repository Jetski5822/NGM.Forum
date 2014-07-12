using NGM.Forum.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.Services
{
    public interface ICountersService : IDependency
    {
        void UpdateForumPartCounters(ThreadPart threadPart);
        void UpdateThreadPartAndForumPartCounters(PostPart postPart);
        void UpdateCounters(PostPart postPart);
    }
    public class CountersService : ICountersService
    {
        private readonly IForumService _forumService;
        private readonly IThreadService _threadService;
        private readonly IPostService _postService;
        private readonly IContentManager _contentManager;
        public CountersService(
            IForumService forumService,
            IThreadService threadService,
            IPostService postService,
            IContentManager contentManager
           )
        {
            _forumService = forumService;
            _threadService = threadService;
            _postService = postService;
            _contentManager = contentManager;
        }

        public void UpdateForumPartCounters(ThreadPart threadPart)
        {
            var commonPart = threadPart.As<CommonPart>();
            if (commonPart != null && commonPart.Record.Container != null)
            {

                var forumPart = threadPart.ForumPart ?? _forumService.Get(commonPart.Record.Container.Id, VersionOptions.Published);

                // TODO: Refactor this to do the count in the DB and not make 3 DB calls.
                var threads = _threadService.Get(forumPart, VersionOptions.Published).Where(t => t.IsInappropriate == false).ToList();
                forumPart.ThreadCount = threads.Count();
                
                var threadIds = threads.Select(t => t.Id).ToList();
                forumPart.PostCount = _contentManager.Query<PostPart, PostPartRecord>(VersionOptions.Published).Where(p => p.IsInappropriate == false)
                                            .Join<CommonPartRecord>().Where(c => threadIds.Contains(c.Container.Id) ).List().Count();
                /*
                 forumPart.ThreadCount = _threadService.Count(forumPart, VersionOptions.Published);
                 forumPart.PostCount = _threadService
                    .Get(forumPart, VersionOptions.Published)
                    .Sum(publishedThreadPart => _postService
                        .Count(publishedThreadPart, VersionOptions.Published));
                 */
                    
            }
        }

        public void UpdateThreadPartAndForumPartCounters(PostPart postPart)
        {
            var commonPart = postPart.As<CommonPart>();
            if (commonPart != null &&
                commonPart.Record.Container != null)
            {

                ThreadPart threadPart = postPart.ThreadPart ??
                                        _threadService.Get(commonPart.Record.Container.Id, VersionOptions.Published);

                //threadPart.PostCount = _postService.Count(threadPart, VersionOptions.Published);
                threadPart.PostCount = _contentManager.Query<PostPart, PostPartRecord>(VersionOptions.Published).Where(p => p.IsInappropriate == false).Join<CommonPartRecord>().Where(c => c.Container.Id == threadPart.Id).List().Count();

                UpdateForumPartCounters(threadPart);
            }
        }

        public void UpdateCounters(PostPart postPart)
        {
            if (postPart.IsParentThread())
                return;

            UpdateThreadPartAndForumPartCounters(postPart);
        }

    }
}