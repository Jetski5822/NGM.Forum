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
        }
    }
}
