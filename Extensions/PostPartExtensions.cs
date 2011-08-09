using NGM.Forum.Models;

namespace NGM.Forum.Extensions {
    public static class PostPartExtensions {
        public static bool IsAnswer(this PostPart postPart) {
            return postPart.MarkedAs == MarkedAs.Answer;
        }
    }
}