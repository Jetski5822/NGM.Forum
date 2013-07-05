using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NGM.Forum.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Security;

namespace NGM.Forum.Services {
    public interface IThreadService : IDependency {
        ThreadPart Get(int id, VersionOptions versionOptions);
        IEnumerable<ThreadPart> Get(ForumPart forumPart);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, VersionOptions versionOptions);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, VersionOptions versionOptions);
        int Count(ForumPart forumPart, VersionOptions versionOptions);
    }

    public class ThreadService : IThreadService {
        private readonly IContentManager _contentManager;

        public ThreadService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public ThreadPart Get(int id, VersionOptions versionOptions) {
            return _contentManager
                .Query<ThreadPart, ThreadPartRecord>(versionOptions)
                .WithQueryHints(new QueryHints().ExpandRecords<AutoroutePartRecord, TitlePartRecord, CommonPartRecord>())
                .Where(x => x.Id == id).List().SingleOrDefault();
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart) {
            return Get(forumPart, VersionOptions.Published);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, VersionOptions versionOptions) {
            return GetParentQuery(forumPart, versionOptions)
                .OrderByDescending(cr => cr.PublishedUtc)
                .ForPart<ThreadPart>()
                .List()
                .ToList();
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count) {
            return Get(forumPart, skip, count, VersionOptions.Published);
        }

        // The order by on this record needs to be revisited.
        public IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, VersionOptions versionOptions) {
            // Initial Order by Sticky Part of query and make sure its for this forum.
            var query = _contentManager
                .HqlQuery<ThreadPart>()
                .ForType("Thread")
                .ForVersion(versionOptions)
                .OrderBy(o => o.ContentPartRecord<ThreadPartRecord>(), x => x.Desc("IsSticky"));

            var queryManager =
                typeof (DefaultHqlQuery<ThreadPart>).GetField("_query", BindingFlags.Instance | BindingFlags.NonPublic)
                                                    .GetValue(query);

            var fiJoins = typeof (DefaultHqlQuery).GetField("_joins", BindingFlags.Instance | BindingFlags.NonPublic);
            var joins = fiJoins.GetValue(queryManager) as List<Tuple<IAlias, Join>>;

            joins.Add(new Tuple<IAlias, Join>(new Alias("Orchard.Core.Common.Models"),
                                              new Join("CommonPartRecord", "cprt", ",")));
            joins.Add(new Tuple<IAlias, Join>(new Alias("Orchard.Core.Common.Models"),
                                              new Join("CommonPartRecord", "cprp", ",")));


            query = query
                .Where(alias => alias.Named("cprt").Property("Container", "tContainer"),
                       factory => factory.Eq("Id", forumPart.ContentItem.Record.Id))
                .Where(alias => alias.Named("cprt"), factory => factory.EqProperty("Id", "threadPartRecord.Id"))
                .Where(alias => alias.Named("cprp").Property("Container", "postContainer"),
                       factory => factory.EqProperty("Id", "cprt.Id"))
                .OrderBy(o => o.Named("cprp"), x => x.Desc("PublishedUtc"));

            /*
             Select tpr.* from NGM_Forum_ThreadPartRecord tpr
inner join Common_CommonPartRecord cprt on cprt.Container_id = 21 -- Correct threads on Forum
										and tpr.Id = cprt.Id
inner join Common_CommonPartRecord cprp on cprp.Container_id = cprt.Id
inner join NGM_Forum_PostPartRecord ppr on ppr.Id = cprp.Id
order by tpr.IsSticky desc, cprp.ModifiedUtc desc

             */

            return query
                .List()
                .Distinct()
                .Skip(skip)
                .Take(count)
                .ToList();
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, IUser user) {
            return GetParentQuery(forumPart, VersionOptions.Published)
                .Where(o => o.OwnerId == user.Id)
                .OrderByDescending(cr => cr.PublishedUtc)
                .ForPart<ThreadPart>()
                .List()
                .ToList();
        }

        public int Count(ForumPart forumPart, VersionOptions versionOptions) {
            return GetParentQuery(forumPart, versionOptions).Count();
        }

        private IContentQuery<CommonPart, CommonPartRecord> GetParentQuery(IContent parentPart, VersionOptions versionOptions) {
            return _contentManager.Query<CommonPart, CommonPartRecord>(versionOptions)
                                  .Where(cpr => cpr.Container == parentPart.ContentItem.Record);
        }
    }
}