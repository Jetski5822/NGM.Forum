using NGM.Forum.Models;

namespace NGM.Forum.Extensions {
    public static class PostExtensions {
        public static bool IsApproved(this PostPart postPart) {
            return postPart.Approved;
        }
    }
}