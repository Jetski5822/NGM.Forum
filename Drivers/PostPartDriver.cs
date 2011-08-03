using System.Collections.Generic;
using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Routable.Models;
using Orchard.ContentManagement;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class PostPartDriver : ContentPartDriver<PostPart> {
        protected override string Prefix {
            get { return "PostPart"; }
        }

        protected override DriverResult Display(PostPart postPart, string displayType, dynamic shapeHelper) {
            var contentShapeResults = new List<ContentShapeResult>(new[] {
                ContentShape("Parts_Posts_Post_Manage",
                    () => shapeHelper.Parts_Posts_Post_Manage(ContentPart: postPart, IsClosed: postPart.ThreadPart.IsClosed)),
                ContentShape("Parts_Posts_Post_Metadata",
                    () => shapeHelper.Parts_Posts_Post_Metadata(ContentPart: postPart, CommonPart: postPart.As<ICommonPart>()))
            });


            if (postPart.IsParentThread())
                return Combined(contentShapeResults.ToArray());

            contentShapeResults.Add(ContentShape("Parts_Posts_Post_Title",
                             () => shapeHelper.Parts_Posts_Post_Title(ContentPart: postPart, CommonPart: postPart.As<ICommonPart>(), RoutePart: postPart.ThreadPart.As<RoutePart>())));

            return Combined(contentShapeResults.ToArray());
        }
    }
}