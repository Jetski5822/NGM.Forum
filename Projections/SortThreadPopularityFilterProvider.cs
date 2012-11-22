//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Contrib.Taxonomies.Models;
//using Contrib.Taxonomies.Services;
//using Orchard.ContentManagement;
//using Orchard.Events;
//using Orchard.Localization;

//namespace NGM.Forum.Projections {
//    public interface ISortCriterionProvider : IEventHandler {
//        void Describe(dynamic describe);
//    }

//    public class SortThreadPopularityFilterProvider : ISortCriterionProvider {
//        private readonly ITaxonomyService _taxonomyService;

//        public SortThreadPopularityFilterProvider(ITaxonomyService taxonomyService) {
//            _taxonomyService = taxonomyService;
//            T = NullLocalizer.Instance;
//        }

//        public Localizer T { get; set; }

//        public void Describe(dynamic describe) {
//            describe.For("Thread", T("Thread"), T("Thread"))
//                .Element("Popularity", T("Popularity of thread"), T("Popularity of thread within forum"),
//                    (Action<dynamic>)ApplyFilter,
//                    (Func<dynamic, LocalizedString>)DisplayFilter,
//                    "SelectPopularityAlgorithm"
//                );
//        }

//        public void ApplyFilter(dynamic context) {
//        }

//        public void ApplySortCriterion(dynamic context) {
//            bool ascending = Convert.ToBoolean(context.State.Algorithm);
            

//            // generate the predicate based on the editor which has been used
//            Action<IHqlExpressionFactory> predicate = y => y.Eq("PropertyName", propertyName);

//            // combines the predicate with a filter on the specific property name of the storage, as implemented in FieldIndexService

//            // apply where clause
//            context.Query = context.Query.Where(relationship, predicate);

//            // apply sort
//            context.Query = ascending
//                ? context.Query.OrderBy(relationship, x => x.Asc("Value"))
//                : context.Query.OrderBy(relationship, x => x.Desc("Value"));
//        }

//        public LocalizedString DisplaySortCriterion(dynamic context) {
//            bool algorithm = Convert.ToBoolean(context.State.Algorithm);

//            return T("Ordered using the {0} popularity algorithm", algorithm);

//        }
//    }

//}