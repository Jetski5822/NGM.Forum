using NGM.Forum.Models;

namespace NGM.Forum.Extensions {
    public static class ThreadExtensions {
        public static bool IsApproved(this ThreadPart threadPart) {
            return threadPart.Approved;
        }
    }
}