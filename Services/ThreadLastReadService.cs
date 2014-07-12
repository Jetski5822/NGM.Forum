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


namespace NGM.Forum.Services {
    public interface IThreadLastReadService : IDependency {
        IEnumerable<ThreadLastReadRecord> GetThreadsReadRecords(int userId, IEnumerable<int> threadIds);
        void MarkThreadAsRead(int userId, int threadId);
        ThreadLastReadRecord GetThreadReadState(int userId, int threadId);
        ThreadPart.ReadStateEnum GetReadState(int userId, ThreadPart part);
    }

    public class ThreadLastReadService : IThreadLastReadService
    {
        private readonly IContentManager _contentManager;
        private readonly IRepository<ThreadLastReadRecord> _threadLastReadRepository;

        public ThreadLastReadService(
            IContentManager contentManager,
            IRepository<ThreadLastReadRecord> threadLastReadRepository
       )
        {
            _contentManager = contentManager;
            _threadLastReadRepository = threadLastReadRepository;
        }

        public IEnumerable<ThreadLastReadRecord> GetThreadsReadRecords(int userId, IEnumerable<int> threadIds)
        {
            //this could be made 'smaller' by only selecting
            var threads = _threadLastReadRepository.Table.Where(e => e.UserId == userId && threadIds.Contains( e.Id)).ToList();
            return threads;
        }

        public ThreadLastReadRecord GetThreadReadState(int userId, int threadId)
        {
            ThreadLastReadRecord returnVal = null;
            var threads = _threadLastReadRepository.Table.Where(e => e.UserId == userId && e.ThreadId == threadId).OrderByDescending( e=>e.LastReadDate).ToList();            
            //newest is at the top so delete the below if necesssary
            if (threads.Count > 0)
            {
                returnVal = threads[0];
                //delete any extra entries.  It is highly unlikely this will happen but a user opening the same thread in two browsers
                //could create a race condition that allows it.  Rather than controlling threads.. just delete in the odd case it happens
                for (int i =1; i < threads.Count; i ++ )
                {
                    _threadLastReadRepository.Delete(threads[i]);
                }
            }
            
            return returnVal;
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

        public ThreadPart.ReadStateEnum GetReadState( int userId, ThreadPart part)
        {
            ThreadPart.ReadStateEnum returnVal = ThreadPart.ReadStateEnum.NOT_SET;

            var lastReadRecord = GetThreadReadState(userId, part.Id);
            if (lastReadRecord == null)
            {
                returnVal = ThreadPart.ReadStateEnum.Unread;
            }
            // the LastestPost will be null if all reply posts have been marked inappropriate
            // In this case, the thread may have been read (before being marked inappropriate) but that cannot be determined            
            else if (part.LatestPost != null)
            {
                if (part.LatestPost.IsPublished() && part.LatestPost.As<CommonPart>().PublishedUtc.Value > lastReadRecord.LastReadDate.Value)
                {
                    returnVal = ThreadPart.ReadStateEnum.NewPosts;
                }
                else
                {
                    returnVal = ThreadPart.ReadStateEnum.ReadNoNewPosts;
                }
            }
            else
            {
                //if all posts have been marked inappropriate and the initial thread has been read (there is a 'lastReadRecord) then
                //show it as read with no new posts since in theory, any replies marked as inappropriate are no long relevant to the admin 
                //(i.e. someone else took care of it).  This is not a *perfect* solution but it is good-enough otherwise the lastpost implementation
                //would need total reworking.
                returnVal = ThreadPart.ReadStateEnum.ReadNoNewPosts; 
            }
            return returnVal;
        }
    }
}