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
        
        void MarkThreadAsRead(int userId, int threadId);

        void SetThreadsReadState(int userId, int forumsHomepageId, IEnumerable<ThreadPart> threadParts);
       
        List<PostPart> GetNewPosts(int forumsHomePageId, int userId, int daysPrior, int pageStartIndex, int pageSize);
        Tuple<Dictionary<int, ThreadPart>, Dictionary<int, List<PostPart>>> GetNewPostsByThread(int forumsHomePageId, int userId, int daysPrior, int pageStartIndex, int pageIndex);
                
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
            //find all threads in the current forum that are newer than the last 'all read date' or 30 days (whichever is newer)          
            // from the second repository get the individual last read dates of the threads read since the last 'all read' date and remove those form the list where appropriate
            //so now have a list of threads with valid new posts
            //get the posts for those threads and filter based on the last 'all ready date'.  
            //for threads in the second repository filter those posts based on its last read date
            
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
                        
            //find all threads in the forums that are candidates for having new posts
            var threads = _contentManager.Query<ThreadPart, ThreadPartRecord>(VersionOptions.Published)
                           .Where(  thread => thread.IsDeleted == false && 
                                    thread.IsInappropriate == false && 
                                    thread.LatestValidPostDate > periodEnd &&
                                    thread.ForumsHomepageId == forumsHomepageId
                                 ).List().ToList();

            //get the list of last read for the individual read threads 
            var threadLastReadDic = _threadLastReadRepository.Table.Where(e => e.UserId == userId).ToDictionary(e => e.ThreadId, e => e.LastReadDate);

            var alreadyReadThreadIds = threads.Where(thread => threadLastReadDic.ContainsKey(thread.Id) && thread.LastestValidPostDate <= threadLastReadDic[thread.Id].Value).Select( thread=>thread.Id).ToList();
            threads.RemoveAll( thread=> alreadyReadThreadIds.Contains( thread.Id ));

            var threadsWithUnreadPosts = threads.Select( thread=>thread.Id).ToList();
            //now have a list of threads that are known to have unread posts.  So need to get posts for those threads and filter them
            var postsByThread = _contentManager.Query<PostPart, PostPartRecord>().ForVersion(VersionOptions.Published)
                .Where(p => p.IsInappropriate == false)
                .WithQueryHints(new QueryHints().ExpandRecords<CommonPartRecord>())
                .Join<CommonPartRecord>()
                .Where(commonPart => threadsWithUnreadPosts.Contains(commonPart.Container.Id)  //filter by the ID
                                     && commonPart.PublishedUtc >= periodEnd  //filter out posts that too old to be candidates to be new posts
                ).List().ToList()
                .OrderBy(p => p.ThreadPart.Id).ToList();

            
            List<int> readPostIds = new List<int>();

            for (int i = 0; i < postsByThread.Count(); i++)
            {
                var post = postsByThread[i];
                DateTime? lastReadDate = null;
                if (threadLastReadDic.TryGetValue(post.As<CommonPart>().Container.Id, out lastReadDate))
                {
                    if (post.As<CommonPart>().PublishedUtc < lastReadDate.Value)
                    {
                        readPostIds.Add(post.Id);
                    }
                }
            }

            postsByThread.RemoveAll(post => readPostIds.Contains(post.Id));

            return postsByThread.Skip(pageStartIndex).Take(pageSize).ToList(); 
        }

        public Tuple<Dictionary<int, ThreadPart>, Dictionary<int, List<PostPart>>> GetNewPostsByThread(int forumsHomepageId, int userId, int daysPrior, int pageStartIndex, int pageSize)
        {

            //get the last date all forums have been markesd as read or the last 30 days whichever is more recent.
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

            //get all threads with new posts based on the last time all forums where marked as read.
            //this is not a perfect selection of unread threads but will limit the query to start with.
            var threads = _contentManager.Query<ThreadPart, ThreadPartRecord>()
                .Where(thread => thread.IsDeleted == false && 
                       thread.IsInappropriate == false && 
                       thread.LatestValidPostDate > periodEnd && 
                       thread.ForumsHomepageId == forumsHomepageId).List().ToList();

            //could just do it with sql var result = _session.CreateSQLQuery("SELECT * FROM Sometable");                       
            var threadIds = threads.Select(t => t.Id).ToList();

            //get the individual threads ids that have beeen read
            var lastReadDic = _threadLastReadRepository.Table.Where(e => e.UserId == userId).ToDictionary(e => e.ThreadId, e => e.LastReadDate);

            //now further filters the threads based on those that are in the repository of individually read threads
            var threadsWithNewPosts = threads.Where(t => ( lastReadDic.ContainsKey(t.Id) && t.LastestValidPostDate > lastReadDic[t.Id].Value) 
                                                           || !lastReadDic.ContainsKey(t.Id)).ToList();


            var threadsWithNewPostsDict = threadsWithNewPosts.Skip(pageStartIndex).Take(pageSize).ToDictionary(t => t.Id, t => t);

            var postsByThread = _contentManager.Query<PostPart, PostPartRecord>().ForVersion(VersionOptions.Published)
                .Where(p => p.IsInappropriate == false)
                .WithQueryHints(new QueryHints().ExpandRecords<CommonPartRecord>())
                .Join<CommonPartRecord>()
                .Where(post => threadsWithNewPostsDict.Keys.Contains(post.Container.Id) && post.PublishedUtc >= periodEnd).List().ToList().OrderBy(p => p.ThreadPart.Id).ToList();

            List<int> readPostIds = new List<int>();

            for (int i = 0; i < postsByThread.Count(); i++)
            {
                var post = postsByThread[i];
                DateTime? lastReadDate = null;
                if (lastReadDic.TryGetValue(post.As<CommonPart>().Container.Id, out lastReadDate))
                {
                    if (post.As<CommonPart>().PublishedUtc < lastReadDate.Value)
                    {
                        readPostIds.Add(post.Id);
                    }
                }
            }

            postsByThread.RemoveAll(post => readPostIds.Contains(post.Id));

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
            return new Tuple<Dictionary<int, ThreadPart>, Dictionary<int, List<PostPart>>>(threadsWithNewPostsDict, threadPostDic);

        }

        private ForumsHomePageLastReadRecord  GetForumsHomePageLastReadRecord( int forumsHomepageId, int userId) {
            return _forumsHomepageLastReadRepository.Table.Where(row => row.ForumsHomePageId == forumsHomepageId && row.UserId == userId).FirstOrDefault();
        }

        private DateTime? GetForumsHomePageLastReadDate(int forumsHomepageId, int userId)
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
                forumsHomePageLastReadDateRecord.LastReadDate = newAllReadDate;
                _forumsHomepageLastReadRepository.Update(forumsHomePageLastReadDateRecord);
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

        private IEnumerable<ThreadLastReadRecord> GetThreadsReadRecords(int userId, IEnumerable<int> threadIds)
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

        public void SetThreadsReadState(int userId, int forumsHomepageId, IEnumerable<ThreadPart> threadParts)
        {            

            List<int> threadIds = threadParts.Select(thread => thread.Id).ToList();
            //var lastReadRecords = _threadLastReadRepository.Table.Where(e => e.UserId == userId && threadIds.Contains(e.ThreadId)).OrderByDescending(e => e.LastReadDate).ToList();

            var lastReadDict = _threadLastReadRepository.Table.Where(e => e.UserId == userId && threadIds.Contains(e.ThreadId)).ToList().ToDictionary( e=>e.ThreadId, e=>e.LastReadDate );

            DateTime? forumsHomepageLastReadDate = null;
            forumsHomepageLastReadDate = GetForumsHomePageLastReadDate(forumsHomepageId, userId);

            if ( forumsHomepageLastReadDate == null ) {
                forumsHomepageLastReadDate = DateTime.UtcNow.AddDays(-30);            
            }

            foreach ( var thread in threadParts ) {

                DateTime? lastReadDate = new DateTime();
                if ( lastReadDict.TryGetValue( thread.Id, out lastReadDate )) {

                    if (thread.LatestPost != null)
                    {
                        if (thread.LatestPost.IsPublished() && thread.LastestValidPostDate > lastReadDate)
                        {
                            thread.ReadState = ThreadPart.ReadStateEnum.NewPosts;
                        }
                        else
                        {
                            thread.ReadState = ThreadPart.ReadStateEnum.ReadNoNewPosts;
                        }
                    }
                    else
                    {
                        //if all posts have been marked inappropriate and the initial thread has been read (there is a 'lastReadRecord) then
                        //show it as read with no new posts since in theory, any replies marked as inappropriate are no long relevant to the admin 
                        //(i.e. someone else took care of it).  This is not a *perfect* solution but it is good-enough otherwise the lastpost implementation
                        //would need total reworking.
                        thread.ReadState = ThreadPart.ReadStateEnum.ReadNoNewPosts;
                    }
                }
                else if (thread.LastestValidPostDate > forumsHomepageLastReadDate)
                {
                    //unread because its not in the last read dict therefore it has never been opened
                    thread.ReadState = ThreadPart.ReadStateEnum.Unread;
                }
                else
                {
                    //everything older than the forumsHomepageLastReadDate is treated a read

                    thread.ReadState = ThreadPart.ReadStateEnum.ReadNoNewPosts;
                }
            }

        }

    }
}