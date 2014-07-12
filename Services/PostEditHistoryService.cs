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
using NGM.Forum.ViewModels;
using Orchard.Users.Models;

namespace NGM.Forum.Services {

    public interface IPostEditHistoryService : IDependency
    {
        void SaveEdit(PostEditHistoryRecord editRecord);
        IEnumerable<PostEditHistoryEntry> GetEdits(int postId);
   
    }

    public class PostEditHistoryService : IPostEditHistoryService
    {
        private readonly IContentManager _contentManager;
        private readonly IRepository<PostEditHistoryRecord> _postEditHistoryRepository;
        private readonly IRepository<UserPartRecord> _userRepository;

        public PostEditHistoryService(
            IContentManager contentManager,
            IRepository<PostEditHistoryRecord> postEditHistoryRepository,
            IRepository<UserPartRecord> userRepository
         )
        {
            _contentManager = contentManager;
            _postEditHistoryRepository = postEditHistoryRepository;
            _userRepository = userRepository;
        }

        public void SaveEdit(PostEditHistoryRecord editRecord)
        {
            _postEditHistoryRepository.Create(editRecord);

        }

        public IEnumerable<PostEditHistoryEntry> GetEdits(int postId)
        {
            var entries = _postEditHistoryRepository.Table.Where(rec => rec.PostId == postId).ToList();
            var userIds = entries.Select(e => e.UserId).ToList();
            var users = _userRepository.Table.Where(user => userIds.Contains(user.Id)).ToDictionary(e => e.Id, e => e.UserName);
            return entries.Select(e => new PostEditHistoryEntry { Text = e.Text, EditedBy = users[e.UserId], Format = e.Format, EditDate = e.EditDate }).OrderBy(e=>e.EditDate);
        }
    }
}