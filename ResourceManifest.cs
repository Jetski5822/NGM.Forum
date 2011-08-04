using Orchard.UI.Resources;

namespace NGM.Forum {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var resourceManifest = builder.Add();
            resourceManifest.DefineStyle("ForumsAdmin").SetUrl("ngm-forum-admin.css");
            resourceManifest.DefineStyle("Forums").SetUrl("forum.css");
        }
    }
}