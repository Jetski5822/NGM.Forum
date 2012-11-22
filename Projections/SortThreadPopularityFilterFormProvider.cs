//using System;
//using System.Collections.Generic;
//using System.Web.Mvc;
//using Contrib.Taxonomies.Helpers;
//using Contrib.Taxonomies.Services;
//using NGM.Forum.Services.Popularity;
//using Orchard.DisplayManagement;
//using Orchard.Events;
//using Orchard.Localization;

//namespace NGM.Forum.Projections {
//    public interface IFormProvider : IEventHandler {
//        void Describe(dynamic context);
//    }

//    public class SortThreadPopularityFilterFormProvider : IFormProvider {
//        private readonly IEnumerable<IPopularityService> _popularityServices;

//        public SortThreadPopularityFilterFormProvider(
//            IShapeFactory shapeFactory,
//            IEnumerable<IPopularityService> popularityServices) {
//            _popularityServices = popularityServices;
//            Shape = shapeFactory;
//            T = NullLocalizer.Instance;
//        }

//        protected dynamic Shape { get; set; }
//        public Localizer T { get; set; }

//        public void Describe(dynamic context) {
//            Func<IShapeFactory, object> form =
//                shape => {
//                    var f = Shape.Form(
//                        _Algorithm: Shape.SelectList(
//                            Id: "AlgorithmName", Name: "AlgorithmName",
//                            Title: T("Algorithm")),
//                        Description: T("The algoritm that will be applied to the results.")
//                        );

//                    foreach (var service in _popularityServices) {
//                        f._Algorithm.Add(new SelectListItem { Value = service.Name, Text = T(service.Name).Text });
//                    }

//                    return f;
//                };

//            context.Form("SelectPopularityAlgorithm", form);
//        }
//    }
//}