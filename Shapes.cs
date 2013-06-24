using System;
using System.Linq;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Localization;

namespace NGM.Forum {
    public class Shapes : IShapeTableProvider {
        public Shapes() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Discover(ShapeTableBuilder builder) {
            builder.Describe("Post_Body_Editor")
                .OnDisplaying(displaying => {
                    string flavor = displaying.Shape.EditorFlavor;
                    displaying.ShapeMetadata.Alternates.Add("Post_Body_Editor__" + flavor);
                });

            // We support multiple forum types, but need to be able to skin forum admin shapes, so add alternates for any content type that has a ForumPart.
            builder.Describe("Content").OnDisplaying(displaying => {
                var content = (ContentItem) displaying.Shape.ContentItem;

                if (content.Parts.All(x => x.PartDefinition.Name != typeof (ForumPart).Name))
                    return;

                var displayType = !String.IsNullOrWhiteSpace(displaying.ShapeMetadata.DisplayType) ? displaying.ShapeMetadata.DisplayType : "Detail";
                var alternates = new[] {
                    String.Format("Content__Forum"),
                    String.Format("Content_{0}__Forum", displayType)
                };

                foreach (var alternate in alternates.Where(alternate => !displaying.ShapeMetadata.Alternates.Contains(alternate))) {
                    displaying.ShapeMetadata.Alternates.Add(alternate);
                }
            });
        }
    }
}
