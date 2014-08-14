using System.Collections.Generic;
using System.Linq;
using System;
using NHibernate;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Security;
using Orchard.Users.Models;
using Orchard.Logging;
using NGM.Forum.ViewModels;
using NGM.Forum.Models;
using System.Web.Mvc;

namespace NGM.Forum.Services {

    public interface IReportPostService : IDependency {
        Tuple<int, IEnumerable<ReportedPostRecordViewModel>> GetPostReports(
            int? postId,
            DateTime? startDate,
            DateTime? endDate,
            string reportedByUserName,
            string reviewedByUserName,
            string reportedUserName,
            bool unresolvedOnly,
            int? Page,
            int? PageSize
       );
        void DeleteReports(List<int> ids);
        void CreateReport(ReportedPostRecord record);
        ReportedPostRecord GetPostReport(int id);

        void UpdateReport(ReportedPostRecord report);

        IQueryable<ReportedPostRecord> Get();
        IQueryable<ReportedPostRecord> GetReportsForPost(int postId);
        ReportPostService.CreatePostResultEnum PostReportStatus(int postId, int reportedByUserId);
    }

    public class ReportPostService : IReportPostService
    {
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IRepository<ReportedPostRecord> _postReportRepository;
        private readonly IRepository<UserPartRecord> _userPartRepository;

        public ILogger Logger { get; set; }            

        public enum CreatePostResultEnum
        {            
            Accepted,
            AlreadyReportedButPendingResolution,
            AlreadyReportByUser,
            CannotBeReportedByCreator
        }

        public ReportPostService(
            IContentManager contentManager, 
            IRepository<ReportedPostRecord> postReportRepository,
            IRepository<UserPartRecord> userPartRepository,
            IOrchardServices orchardServices

        )
        {
            _contentManager = contentManager;
            _postReportRepository = postReportRepository;
            _userPartRepository = userPartRepository;
            _orchardServices = orchardServices;
            Logger = NullLogger.Instance;

        }

        public void DeleteReports( List<int> ids ){
            var deleteList= _postReportRepository.Table.Where( post=> ids.Contains( post.PostId )).ToList();
            foreach( var report in deleteList ){
                //in case of simultaneous deletion by two users, catch any error resulting from the record being non-existent and continue deleting
                try{
                    _postReportRepository.Delete( report);
                } catch ( Exception e )  {
                    Logger.Error(e, "Error deleting post report in NGM.Forum module.");
                }
            }
        }

        public void  UpdateReport(ReportedPostRecord report )
        {
            
            _postReportRepository.Update(report);

        }

        /// <summary>
        /// Gets the status of the post verus existing inappropriate reports. 
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="reportedByUserId"></param>
        /// <returns></returns>
        public CreatePostResultEnum PostReportStatus( int postId, int reportedByUserId ) {

            var existingRecord = _postReportRepository.Table.Where(p => p.PostId == postId).ToList();

            var post = _orchardServices.ContentManager.Get( postId, VersionOptions.Published );

            CreatePostResultEnum wasAccepted = CreatePostResultEnum.Accepted;

            if (post.As<CommonPart>().Owner.Id == reportedByUserId)
            {
                wasAccepted = CreatePostResultEnum.CannotBeReportedByCreator;
            }
            else if (existingRecord.Count == 0  )
            {
                //this post has never been reported before so automatically accept the report
                wasAccepted = CreatePostResultEnum.Accepted;                

            } else {
                
                //post has been reported previously, but may or may not have been resolved or may have been reported again after being resolved.

                //if is reported but the resolution is pending don't accept the new report.
                if (existingRecord.Where(p => p.ReportedByUserId == reportedByUserId).Count() > 0)
                {
                    //if this user has already made a report reject any further reports, whether they are resolved or not
                    //don't want a disgruntled user to keep reporting the same post (i.e. an argument between users)
                    //If the report is truly inappropriate, another user will report it again
                    wasAccepted = CreatePostResultEnum.AlreadyReportByUser;
                } 
                else if (existingRecord.Where(report => report.IsResolved == false).Count() > 0)
                {   //is reported and pending resolution
                    wasAccepted = CreatePostResultEnum.AlreadyReportedButPendingResolution;
                }
                else if (existingRecord.Where(report => report.IsResolved == true).Count() > 0)
                {    //this report has already been reported and resolved but is being re-reported
                    wasAccepted = CreatePostResultEnum.Accepted;
                }

            }

            return wasAccepted;
        }

        public void CreateReport(ReportedPostRecord record)
        {


            //check if this post has already been reported
            var existingRecord = _postReportRepository.Table.Where(post => post.PostId == record.PostId).ToList();

            record.ReportedDate = DateTime.UtcNow;

            //This follows the same logic as the 'PostReportStatus' above so that posts are only recorded if appropriate
            //Yes, the logic could be simplified here but will model the above function exactly to make keeping the logic verifiable and in sync easier.
            if (existingRecord.Count == 0  )
            {
                //its a new report so automatically accept it
                _postReportRepository.Create(record);

            } else {
                

                if (existingRecord.Where(p => p.ReportedByUserId == record.ReportedByUserId).Count() > 0)
                {
                    //this user has already reported the post, so don't accept a second submission. 
                    //This will stop a disgruntled user from repeatedly reporting the same post.  
                    //If the post is truly inappropriate someone else will report it.
                } 
                else if (existingRecord.Where(report => report.IsResolved == false).Count() > 0)
                {
                    //is reported and pending resolution so don't make a new entry
                }
                else if (existingRecord.Where(report => report.IsResolved == true).Count() > 0)
                {    //this report has already been reported and resolved but is being re-reported
                    _postReportRepository.Create(record);
                }
            }
        }

        public IQueryable<ReportedPostRecord> Get()
        {
            return _postReportRepository.Table;
        }

        public IQueryable<ReportedPostRecord> GetReportsForPost(int postId)
        {
            return _postReportRepository.Table.Where(r => r.PostId == postId);
        }

        public ReportedPostRecord GetPostReport( int id ){
            return _postReportRepository.Get( id );
        }


        public Tuple<int, IEnumerable<ReportedPostRecordViewModel>> GetPostReports( 
            int? postId,
            DateTime? startDate, 
            DateTime? endDate,
            string reportedByUserName,            
            string reviewedByUserName,
            string reportedUserName,
            bool unresolvedOnly,
            int? page,
            int? pageSize
       ) {


            var reportedPosts = _postReportRepository.Table;

            //apply the filters to the repository
            if (postId != null)
            {
                reportedPosts = reportedPosts.Where(r => r.PostId == postId );
            }

            if ( startDate != null ) {
                reportedPosts = reportedPosts.Where( r=>r.ReportedDate >= startDate );
            }

            if ( endDate != null ) {
                //up to the end of the selected day
                reportedPosts =  reportedPosts.Where(r => r.ReportedDate <= endDate.Value.Date.AddDays(1).AddSeconds(-1));
            }

            Dictionary<int, string> reportedByUserNames = new Dictionary<int, string>();
            if (!String.IsNullOrWhiteSpace(reportedByUserName))
            {
                reportedByUserNames = _contentManager.Query<UserPart, UserPartRecord>().Where(user => user.NormalizedUserName.Contains(reportedByUserName)).List().Select(i => new { UserName = i.UserName, Id = i.Id }).ToDictionary(i => i.Id, i => i.UserName);
                reportedPosts = reportedPosts.Where(r => reportedByUserNames.Keys.Contains(r.ReportedByUserId));
            }

            Dictionary<int, string> reviewedByUserNames = new Dictionary<int, string>();
            if (!String.IsNullOrWhiteSpace(reviewedByUserName))
            {
                reviewedByUserNames = _contentManager.Query<UserPart, UserPartRecord>().Where(user => user.NormalizedUserName.Contains(reviewedByUserName)).List().Select(i => new { UserName = i.UserName, Id = i.Id }).ToDictionary(i => i.Id, i => i.UserName);
                reportedPosts = reportedPosts.Where(r => reviewedByUserNames.Keys.Contains(r.ResolvedByUserId));
            }

            Dictionary<int, string> reportedUserNames = new Dictionary<int, string>();
            if (!String.IsNullOrWhiteSpace(reportedUserName))
            {
                reportedUserNames = _contentManager.Query<UserPart, UserPartRecord>().Where(user => user.NormalizedUserName.Contains(reportedUserName)).List().Select(i => new { UserName = i.UserName, Id = i.Id }).ToDictionary(i => i.Id, i => i.UserName);
                reportedPosts = reportedPosts.Where(r => reportedUserNames.Keys.Contains(r.PostedByUserId));
            }

            if (unresolvedOnly)
            {
                reportedPosts = reportedPosts.Where(r => r.IsResolved == false);
            }

            int totalCount = reportedPosts.Count();
            var list = reportedPosts.Skip(page.Value).Take(pageSize.Value).ToList(); ;

            var userNameMapping = new List<int>();

            //lookup the user names that are in the resulting list.  This may be a duplicate lookup of the filtered names but not going to worry about it.
            //this would have been more efficient if the reports used the user name instead of Id but userd Id so in case the name were to change
            var reportedByIds = list.Select(l => l.ReportedByUserId).ToList();
            userNameMapping = list.Select(l => l.ReportedByUserId).ToList();
            //var reportedBy = _contentManager.Query<UserPart, UserPartRecord>().Where(user => reportedByIds.Contains(user.Id)).List().ToList().Select(i => new { UserName = i.UserName, Id = i.Id }).ToDictionary(i => i.Id, i => i.UserName);

            var reportedIds = list.Select(l => l.PostedByUserId).ToList();
            userNameMapping.AddRange(reportedIds);         
            //var reported = _contentManager.Query<UserPart, UserPartRecord>().Where(user => reportedIds.Contains(user.Id)).List().ToList().Select(i => new { UserName = i.UserName, Id = i.Id }).ToDictionary(i => i.Id, i => i.UserName);

            var resolvedByIds = list.Select( r=> r.ResolvedByUserId ).ToList();
            userNameMapping.AddRange(resolvedByIds);
            //var resolvedBy = _contentManager.Query<UserPart, UserPartRecord>().Where(user => resolvedByIds.Contains(user.Id)).List().Select(i => new { UserName = i.UserName, Id = i.Id }).ToDictionary(i => i.Id, i => i.UserName);

            userNameMapping = userNameMapping.Distinct().ToList();

            var userMappingDict = _contentManager.Query<UserPart, UserPartRecord>().Where(user => userNameMapping.Contains(user.Id)).List().Select(i => new { UserName = i.UserName, Id = i.Id }).ToDictionary(i => i.Id, i => i.UserName);

            var resultx = list.ToList().Select(a => new ReportedPostRecordViewModel
            {
                ReportId = a.Id,
                PostId = a.PostId,
                IsResolved= a.IsResolved,                                 
                ResolvedDate = a.ResolvedDate,
                ReportedDate = a.ReportedDate,                
                PostByUserId = a.PostedByUserId,
                PostByUserName = a.ReportedByUserId == 0 ? null : userMappingDict[a.PostedByUserId],
                ResolvedByUserId = a.ResolvedByUserId,
                ResolvedByUserName = a.ResolvedByUserId == 0 ? null : userMappingDict[a.ResolvedByUserId],                
                ReportedByUserId = a.ReportedByUserId,
                ReportedByUserName = a.ReportedByUserId == 0 ? null : userMappingDict[a.ReportedByUserId],
                ReasonReported = a.ReasonReported,
                Note = a.Note, 
            }).ToList().OrderByDescending( date=>date.ReportedDate);

            var content = _contentManager.GetMany<PostPart>(resultx.Select(x => x.PostId).Distinct().ToList(), VersionOptions.Latest, QueryHints.Empty).ToDictionary(p => p.Id, p => p);

            //if the reported post has been deleted from the system elsewhere, leave teh report record for historic purposes 
            //so that users with a bad history can still be identified.
            //An alternative implementation would be to remove the history when the post is removed
            //(commented out below and in the post part handler -- untested
            List<int> reportsToDelete = new List<int>();
            foreach (var i in resultx)
            {
                PostPart postFound = null; 
                if (content.TryGetValue( i.PostId, out postFound ) )
                {
                    i.Post = content[i.PostId];
                }
                /*
                else
                {
                    reportsToDelete.Add(i.PostId);
                }
                 */
            }

            /*
            foreach( int i in reportsToDelete){
                resultx.RemoveAt(i);                
            }
            this.DeleteReports(reportsToDelete);
            */

            return new Tuple<int, IEnumerable<ReportedPostRecordViewModel>>(totalCount, resultx);
            /*
             * //this would have been easier if orchard repositories allowed this type of join :(
             * var results = (from  report in _postReportRepository.Table
                         join reportedByUser in _userPartRepository.Table on report.ReportedByUserId equals reportedByUser.Id into reportedbu
                          from xxx in reportedbu.DefaultIfEmpty()
                          join reviewedByUser in _userPartRepository.Table on report.ResolvedByUserId equals reviewedByUser.Id into reviewedbu
                          from yyy in reviewedbu.DefaultIfEmpty()
                          join postedByUser in _userPartRepository.Table on report.PostedByUserId equals postedByUser.Id into pbu
                          from zzz in pbu.DefaultIfEmpty()
                         select new ReportedPostRecordViewModel { Id = report.Id, 
                                                                IsResolved= report.IsResolved, 
                                                                Note = report.Note, 
                                                                PostByUserId = report.PostedByUserId, 
                                                                PostId = report.PostId,
                                                                ReportedByUserId = report.ReportedByUserId,
                                                                ReportedDate = report.ReportedDate,
                                                                ResolvedByUserId = report.ResolvedByUserId,
                                                                PostByUserName = zzz.UserName,
                                                                ResolvedByUserName = yyy.UserName,
                                                                ResolvedDate = report.ResolvedDate,
                                                                ReportedByUserName = xxx.UserName } ).AsQueryable();
             */


           
        }


   
    }

}