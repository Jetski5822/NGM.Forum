using System.Collections.Generic;
using System.Linq;
using NGM.Forum.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using System;
using Orchard.ContentManagement.Aspects;
using System.Linq;


namespace NGM.Forum.Services {

    public interface IThreadLastReadService : IDependency {
        IEnumerable<ThreadLastReadRecord> GetThreadsReadRecords(int userId, IEnumerable<int> threadIds);
        void MarkThreadAsRead(int userId, int threadId);

        void GetThreadReadState(int userId, int forumsHomepageId, IEnumerable<ThreadPart> threadParts);
       
        List<PostPart> GetNewPosts(int forumsHomePageId, int userId, int daysPrior, int pageStartIndex, int pageSize);
        Tuple<Dictionary<int, ThreadPart>, Dictionary<int, List<PostPart>>> GetNewPostsByThread(int forumsHomePageId, int userId, int daysPrior, int pageStartIndex, int pageIndex);
        ForumsHomePageLastReadRecord GetForumsHomePageLastReadRecord(int forumsHomepageId, int userId);
        DateTime? GetForumsHomePageLastReadDate(int forumsHomepageId, int userId);
        void MarkAllRead(int forumsHomeId, int userId);
    }

    public class ThreadLastReadService : IThreadLastReadService
    {

        //READ STATE WILL BE TRACKED USING TWO REPOSITORIES.  
        //One repository (threadLastRead) stores the last read date for a particular forum.  
        //   ->Any thread without a matching record is therefore unread.
        //   ->Any posts in a thread newer than the given date are unread.
        //The other repository (forumsHomePageLastRead) stores the last 'all read date' for the user.
        //   - when a user select 'mark all read' all posts before that date are concidered read.
        //   - any entries in the first repository can be removed at that point (i.e. self maintaining)

        private readonly IContentManager _contentManager;
        private readonly IRepository<ThreadLastReadRecord> _threadLastReadRepository;
        private readonly IRepository<ForumsHomePageLastReadRecord> _forumsHomepageLastReadRepository;
        private readonly IThreadService _threadService;

        public ThreadLastReadService(
            IContentManager contentManager,
            IRepository<ThreadLastReadRecord> threadLastReadRepository,
            IRepository<ForumsHomePageLastReadRecord> forumsHomepageLastReadRepository,
            IThreadService threadService
       )
        {
            _contentManager = contentManager;
            _threadLastReadRepository = threadLastReadRepository;
            _forumsHomepageLastReadRepository = forumsHomepageLastReadRepository;
            _threadService = threadService;
        }

        public List<PostPart> GetNewPosts(int forumsHomepageId, int userId, int daysPrior, int pageStartIndex, int pageSize)
        {

            var forumLastReadDate = GetForumsHomePageLastReadDate(forumsHomepageId, userId);
            if (daysPrior > 30) daysPrior = 30; //ensure this number is realistic to stop huge queries
            var periodEnd = DateTime.UtcNow.AddDays(daysPrior * -1);
            if (forumLastReadDate.HasValue)
            {
                if (forumLastReadDate.Value > periodEnd)
                {
                    periodEnd = forumLastReadDate.Value;
                }
            }
            // lookup every thread for the current forum 
            //the following is extremely inefficient... need to implement a better approach if required.  Get it working first .. 
            //var categories = _contentManager.Query<ForumCategoryPart, ForumCategoryPartRecord>().Join<CommonPartRecord>().Where( cat=>cat.Container.Id == forumsHomepageId).List().Select( t=>t.Id).ToList();
            //var forums = _contentManager.Query<ForumPart, ForumPartRecord>().Join<CommonPartRecord>().Where( forum=>categories.Contains( forum.Container.Id )).List().Select( forum=>forum.Id).ToList();

            

            //need to find which threads belong to this forum. This could be an extremely large number of Ids                        
            var threads = _contentManager.Query<ThreadPart, ThreadPartRecord>(VersionOptions.Published)
                           .Where(  thread => thread.IsDeleted == false && 
                                    thread.IsInappropriate == false && 
                                    thread.LatestValidPostDate >= periodEnd &&
                                    thread.ForumsHomepageId == forumsHomepageId
                                 ).List().Select(thread => thread.Id).ToList();

            var postsByThread = _contentManager.Query<PostPart, PostPartRecord>().ForVersion(VersionOptions.Published)
                .Where(p => p.IsInappropriate == false)
                .WithQueryHints(new QueryHints().ExpandRecords<CommonPartRecord>())
                .Join<CommonPartRecord>()
                .Where(commonPart => threads.Contains(commonPart.Container.Id) && commonPart.PublishedUtc > periodEnd).List().ToList()
                .OrderBy(p => p.ThreadPart.Id).ToList();


            var lastReadDic = _threadLastReadRepository.Table.Where( e=>e.UserId == userId ).ToDictionary( e=>e.ThreadId, e=>e.LastReadDate);

            foreach (var post in postsByThread)
            {
                DateTime? lastReadDate =  null;
                if (lastReadDic.TryGetValue(post.As<CommonPart>().Container.Id, out lastReadDate))
                {
                    if (post.As<CommonPart>().PublishedUtc > lastReadDate.Value)
                    {
                        postsByThread.Remove(post);
                    }
                }                              
            }
            return postsByThread.Skip(pageStartIndex).Take(pageSize).ToList(); 
        }

        public Tuple<Dictionary<int, ThreadPart>, Dictionary<int, List<PostPart>>> GetNewPostsByThread(int forumsHomepageId, int userId, int daysPrior, int pageStartIndex, int pageSize)
        {

            var forumLastReadDate = GetForumsHomePageLastReadDate(forumsHomepageId, userId);

            if (daysPrior > 30) daysPrior = 30; //ensure this number is realistic to stop huge queries
            var periodEnd = DateTime.UtcNow.AddDays(daysPrior * -1);
            if (forumLastReadDate.HasValue)
            {
                if (forumLastReadDate.Value > periodEnd)
                {
                    periodEnd = forumLastReadDate.Value;
                }
            }

            var threads = _contentManager.Query<ThreadPart, ThreadPartRecord>()
                .Where(thread => thread.IsDeleted == false && 
                       thread.IsInappropriate == false && 
                       thread.LatestValidPostDate >= periodEnd && 
                       thread.ForumsHomepageId == forumsHomepageId).List().ToList();

            //or could just do it with sql
            //var result = _session.CreateSQLQuery("SELECT * FROM Sometable");                       
            var threadIds = threads.Select(t => t.Id).ToList();
            var lastReadDic = _threadLastReadRepository.Table.Where(e => e.UserId == userId).ToDictionary(e => e.ThreadId, e => e.LastReadDate);

            var threadsWithNewPosts = threads.Where(t => 
                (lastReadDic.ContainsKey(t.Id) && t.LastestValidPostDate > lastReadDic[t.Id].Value) 
                || !lastReadDic.ContainsKey(t.Id)).ToList();


            var threadDict = threadsWithNewPosts.Skip(pageStartIndex).Take(pageSize).ToDictionary(t => t.Id, t => t);

            var postsByThread = _contentManager.Query<PostPart, PostPartRecord>()
                .Where(p => p.IsInappropriate == false)
                .WithQueryHints(new QueryHints().ExpandRecords<CommonPartRecord>())
                .Join<CommonPartRecord>()
                .Where(post => threadIds.Contains(post.Container.Id) && post.PublishedUtc >= periodEnd).List().ToList().OrderBy(p => p.ThreadPart.Id).ToList();


            Dictionary<int, List<PostPart>> threadPostDic = new Dictionary<int, List<PostPart>>();

            foreach (var thread in threadsWithNewPosts)
            {
                threadPostDic.Add(thread.Id, new List<PostPart>());
            }


            foreach (var post in postsByThread)
            {
                threadPostDic[post.As<ICommonPart>().Container.Id].Add(post);
            }

            //var threadList = _contentManager.GetMany<ThreadPart>(threadIds, VersionOptions.Published, QueryHints.Empty).Where( t=>t.IsInappropriate == false).ToList();
            return new Tuple<Dictionary<int, ThreadPart>, Dictionary<int, List<PostPart>>>(threadDict, threadPostDic);

        }

        public ForumsHomePageLastReadRecord  GetForumsHomePageLastReadRecord( int forumsHomepageId, int userId) {
            return _forumsHomepageLastReadRepository.Table.Where(row => row.ForumsHomePageId == forumsHomepageId && row.UserId == userId).FirstOrDefault();
        }

        public DateTime? GetForumsHomePageLastReadDate(int forumsHomepageId, int userId)
        {
            //can cache this later for optimization
            DateTime? lastRead = null;
            var rec  = GetForumsHomePageLastReadRecord( forumsHomepageId, userId );
            if (rec != null)
            {
                lastRead = rec.LastReadDate;
            }
            return lastRead;
        }



        public void MarkAllRead(int forumsHomepageId, int userId)
        {
            DateTime newAllReadDate = DateTime.UtcNow;
            var forumsHomePageLastReadDateRecord = GetForumsHomePageLastReadRecord(forumsHomepageId, userId);
            if (forumsHomePageLastReadDateRecord != null)
            {              
            } else {
                _forumsHomepageLastReadRepository.Create( new ForumsHomePageLastReadRecord{ LastReadDate = newAllReadDate, UserId = userId, ForumsHomePageId = forumsHomepageId } );
            }

            //if there are existing last read records at the thread level, find and delete them. The last read at the forums home page level will take priority.
            var lastReadThreads = _threadLastReadRepository.Table.Where(e => e.UserId == userId ).ToList();
            var lastReadThreadIds = lastReadThreads.Select(thread => thread.ThreadId).ToList();
            var threadIdsFromCurrentForum = _contentManager.Query<ThreadPart, ThreadPartRecord>().Where(thread => thread.ForumsHomepageId == forumsHomepageId && lastReadThreadIds.Contains(thread.Id)).List().Select(thread => thread.Id).ToList();
            foreach (var thread in lastReadThreads)
            {
                if ( threadIdsFromCurrentForum.Contains( thread.ThreadId )) {
                    _threadLastReadRepository.Delete( thread );
                }
            }               
        }

        public IEnumerable<ThreadLastReadRecord> GetThreadsReadRecords(int userId, IEnumerable<int> threadIds)
        {

            var threads = _threadLastReadRepository.Table.Where(e => e.UserId == userId && threadIds.Contains( e.Id)).ToList();
            return threads;
        }

        public void MarkThreadAsRead(int userId, int threadId)
        {      
            //check there if there is an existing entry and if so reuse it
            var existingEntry = _threadLastReadRepository.Table.Where(e => e.UserId == userId && e.ThreadId == threadId).FirstOrDefault();

            if (existingEntry != null)
            {
                existingEntry.LastReadDate = DateTime.UtcNow;
                _threadLastReadRepository.Update( existingEntry);
            }
            else
            {
                ThreadLastReadRecord rec = new ThreadLastReadRecord
                {
                    ThreadId = threadId,
                    UserId = userId,
                    LastReadDate = DateTime.UtcNow,
                };
                _threadLastReadRepository.Create( rec);
            }         
        }

        public void GetThreadReadState(int userId, int forumsHomepageId, IEnumerable<ThreadPart> threadParts)
        {            

            List<int> threadIds = threadParts.Select(thread => thread.Id).ToList();
            //var lastReadRecords = _threadLastReadRepository.Table.Where(e => e.UserId == userId && threadIds.Contains(e.ThreadId)).OrderByDescending(e => e.LastReadDate).ToList();
            var lastReadRecords = _threadLastReadRepository.Table.Where(e => e.UserId == userId && threadIds.Contains(e.ThreadId)).ToList();
            if (lastReadRecords != null)
            {
                threadParts.Select(thread => thread.ReadState = ThreadPart.ReadStateEnum.Unread);
                foreach (var lastReadRecord in lastReadRecords)
                {
                    var threadPart = threadParts.Where(thread => thread.Id == lastReadRecord.ThreadId).First();

                    // the LastestPost will be null if all reply posts have been marked inappropriate
                    // In this case, the thread may have been read (before being marked inappropriate) but that cannot be determined            
                    if (threadPart.LatestPost != null)
                    {
                        if (threadPart.LatestPost.IsPublished() && threadPart.LatestPost.As<CommonPart>().PublishedUtc.Value > lastReadRecord.LastReadDate.Value)
                        {
                            threadPart.ReadState = ThreadPart.ReadStateEnum.NewPosts;
                        }
                        else
                        {
                            threadPart.ReadState = ThreadPart.ReadStateEnum.ReadNoNewPosts;
                        }
                    }
                    else
                    {
                        //if all posts have been marked inappropriate and the initial thread has been read (there is a 'lastReadRecord) then
                        //show it as read with no new posts since in theory, any replies marked as inappropriate are no long relevant to the admin 
                        //(i.e. someone else took care of it).  This is not a *perfect* solution but it is good-enough otherwise the lastpost implementation
                        //would need total reworking.
                        threadPart.ReadState = ThreadPart.ReadStateEnum.ReadNoNewPosts;
                    }
                }
            }
            else //no read threads where found .. so use the 'mark all date' from the forums home page 
            {
                DateTime? forumsHomepageLastReadDate = null;
                forumsHomepageLastReadDate = GetForumsHomePageLastReadDate(forumsHomepageId, userId);

                if (forumsHomepageLastReadDate.HasValue)
                {
                    foreach (var threadPart in threadParts)
                    {
                        if (threadPart.LastestValidPostDate > forumsHomepageLastReadDate)
                        {
                            threadPart.ReadState = ThreadPart.ReadStateEnum.NewPosts;
                        }
                    }
                }
                else
                {
                    //there is no last read at the forums home page level nor last read on any threads, so everything is unread
                }

            }
        }
    }
}