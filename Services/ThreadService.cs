using System;
using System.Collections.Generic;
using System.Linq;
using Contrib.Voting.Services;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;
using Orchard.Core.Title.Models;

namespace NGM.Forum.Services {
    public interface IThreadService : IDependency {
        ThreadPart Get(ForumPart forumPart, string slug, VersionOptions versionOptions);
        ContentItem Get(int id, VersionOptions versionOptions);
        IEnumerable<ThreadPart> Get(ForumPart forumPart);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, VersionOptions versionOptions);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, VersionOptions versionOptions);

        void CloseThread(ThreadPart threadPart);
        void OpenThread(ThreadPart threadPart);

        double CalculatePopularity(ThreadPart threadPart);
    }

    public class ThreadService : IThreadService {
        private readonly IContentManager _contentManager;
        private readonly IPostService _postService;
        private readonly IVotingService _votingService;

        public ThreadService(IContentManager contentManager, 
            IPostService postService,
            IVotingService votingService) {
            _contentManager = contentManager;
            _postService = postService;
            _votingService = votingService;
        }

        public ThreadPart Get(ForumPart forumPart, string slug, VersionOptions versionOptions) {
            var threadPath = forumPart.As<IRoutableAspect>().GetChildPath(slug);

            return _contentManager.Query<ThreadPart, ThreadPartRecord>()
                .Join<AutoroutePartRecord>()
                .WithQueryHints(new QueryHints().ExpandRecords<TitlePartRecord>())
                .Where(rr => rr.DisplayAlias == path)

            return _contentManager.Query(versionOptions, ContentPartConstants.Thread).Join<AutoroutePartRecord>().Where(rr => rr.Path == threadPath).
                    Join<CommonPartRecord>().Where(cr => cr.Container == forumPart.Record.ContentItemRecord).List().
                    SingleOrDefault().As<ThreadPart>();
        }

        public ContentItem Get(int id, VersionOptions versionOptions) {
            return _contentManager.Get(id, versionOptions);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart) {
            return Get(forumPart, VersionOptions.Published);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, VersionOptions versionOptions) {
            return GetForumQuery(forumPart, versionOptions).List().Select(ci => ci.As<ThreadPart>());
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count) {
            return Get(forumPart, skip, count, VersionOptions.Published);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, VersionOptions versionOptions) {
            return GetForumQuery(forumPart, versionOptions).Slice(skip, count).ToList().Select(ci => ContentExtensions.As<ThreadPart>(ci));
        }

        public void CloseThread(ThreadPart threadPart) {
            threadPart.IsClosed = true;
        }

        public void OpenThread(ThreadPart threadPart) {
            threadPart.IsClosed = false;
        }

        public double CalculatePopularity(ThreadPart threadPart) {
            double questionScore = 0D;
            IList<double> answerScores = new List<double>();

            var posts = _postService.Get(threadPart).ToArray();

            var question = posts.Where(o => o.IsParentThread()).First();
            var questionScoreRecord = _votingService.Get(vote => vote.ContentItemRecord == question.Record.ContentItemRecord && vote.Dimension == VotingConstants.RatingConstant).FirstOrDefault();
            if (questionScoreRecord != null) {
                questionScore = questionScoreRecord.Value;

                foreach (var answer in posts.Where(o => !o.IsParentThread())) {
                    var answerScoreRecord = _votingService.Get(vote => vote.ContentItemRecord == answer.Record.ContentItemRecord && vote.Dimension == VotingConstants.RatingConstant).FirstOrDefault();
                    if (answerScoreRecord != null)
                        answerScores.Add(answerScoreRecord.Value);
                }
            }

            var resultRecord = _votingService.GetResult(threadPart.ContentItem.Id, "count", VotingConstants.ViewConstant);
            var totalViews = resultRecord == null ? 0 : (int)resultRecord.Value;

            var threadCreatedDate = threadPart.As<ICommonPart>().CreatedUtc;
            var threadModifiedDate = threadPart.As<ICommonPart>().ModifiedUtc;

            var top = ((Math.Log(totalViews) * 4) + ((threadPart.PostCount * questionScore) / 5) + answerScores.Sum());
            var bottom = Math.Pow(Convert.ToDouble((threadCreatedDate.GetValueOrDefault(DateTime.Now).AddHours(1).Hour) - ((threadCreatedDate.GetValueOrDefault(DateTime.Now).Subtract(threadModifiedDate.GetValueOrDefault(DateTime.Now))).Hours / 2)), 1.5);

            return top / bottom;
        }

        private IContentQuery<ContentItem, CommonPartRecord> GetForumQuery(ContentPart<ForumPartRecord> forum, VersionOptions versionOptions) {
            return
                _contentManager.Query(versionOptions, ContentPartConstants.Thread).Join<CommonPartRecord>().Where(
                    cr => cr.Container == forum.Record.ContentItemRecord).OrderByDescending(cr => cr.CreatedUtc);
        }
    }
}