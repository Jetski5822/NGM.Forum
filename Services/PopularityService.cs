using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Contrib.Voting.Services;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;

namespace NGM.Forum.Services {
    public interface IPopularityService : IDependency {
        double Calculate(ThreadPart thread);
        string Name { get; }
    }

    public class PopularityService : IPopularityService {
        private readonly IPostService _postService;
        private readonly IVotingService _votingService;

        public PopularityService(IPostService postService,
            IVotingService votingService) {
            _postService = postService;
            _votingService = votingService;
        }

        public double Calculate(ThreadPart thread) {
            double questionScore = 0D;
            IList<double> answerScores = new List<double>();

            var posts = _postService.Get(thread).ToArray();

            var question = posts.First(o => o.IsParentThread());
            var questionScoreRecord = _votingService.Get(vote => vote.ContentItemRecord == question.Record.ContentItemRecord && vote.Dimension == Constants.Voting.RatingConstant).FirstOrDefault();
            if (questionScoreRecord != null) {
                questionScore = questionScoreRecord.Value;

                foreach (var answer in posts.Where(o => !o.IsParentThread())) {
                    PostPart internalAnswer = answer;
                    var answerScoreRecord = _votingService.Get(vote => vote.ContentItemRecord == internalAnswer.Record.ContentItemRecord && vote.Dimension == Constants.Voting.RatingConstant).FirstOrDefault();
                    if (answerScoreRecord != null)
                        answerScores.Add(answerScoreRecord.Value);
                }
            }

            var resultRecord = _votingService.GetResult(thread.ContentItem.Id, "count", Constants.Voting.ViewConstant);
            var totalViews = resultRecord == null ? 0 : (int)resultRecord.Value;

            var threadCreatedDate = thread.As<ICommonPart>().CreatedUtc;
            var threadModifiedDate = thread.As<ICommonPart>().ModifiedUtc;

            var top = ((Math.Log(totalViews) * 4) + ((thread.PostCount * questionScore) / 5) + answerScores.Sum());
            var bottom = Math.Pow(Convert.ToDouble((threadCreatedDate.GetValueOrDefault(DateTime.Now).AddHours(1).Hour) - ((threadCreatedDate.GetValueOrDefault(DateTime.Now).Subtract(threadModifiedDate.GetValueOrDefault(DateTime.Now))).Hours / 2)), 1.5);

            return top / bottom;
        }

        public string Name { get { return "Stack Overflow Algorithm"; } }
    }
}