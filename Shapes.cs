//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using Orchard;
//using Orchard.ContentManagement;
//using Orchard.DisplayManagement.Descriptors;

//namespace NGM.Forum {
//    public class Shapes : IShapeTableProvider {
//        private readonly IWorkContextAccessor _workContextAccessor;

//        public Shapes(IWorkContextAccessor workContextAccessor) {
//            _workContextAccessor = workContextAccessor;
//        }

//        public void Discover(ShapeTableBuilder builder) {
//            builder.Describe("Content")
//                .OnDisplaying(displaying => {
//                                    ContentItem contentItem = displaying.Shape.ContentItem;
//                                    if (contentItem != null) {
//                                        displaying.ShapeMetadata.Alternates.Add("Content_Edit__Thread");
//                                    }
//                              });
//        }
//    }
//}