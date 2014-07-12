using Orchard.UI.Resources;

namespace NGM.Forum {
    public class ResourceManifest : IResourceManifestProvider {
        public void BuildManifests(ResourceManifestBuilder builder) {
            var resourceManifest = builder.Add();
            resourceManifest.DefineStyle("ForumsAdmin").SetUrl("ngm-forum-admin.css");

            resourceManifest.DefineStyle("ThreadsAdmin").SetUrl("ngm-thread-admin.css");

            resourceManifest.DefineStyle("Forum").SetUrl("ngm-forum.css");

            resourceManifest.DefineScript("MomentJs").SetUrl("momentjs/moment-with-langs.min.js", "momentjs/moment-with-langs.js");

            resourceManifest.DefineScript("UTCToLocal").SetUrl("UTCtoLocal.js").SetDependencies("jQuery", "MomentJs");
        }
    }
}