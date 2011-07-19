using Orchard.UI.Resources;

namespace NGM.Forum {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            builder.Add().DefineStyle("ForumsAdmin").SetUrl("orchard-forum-admin.css");
        }
    }
}