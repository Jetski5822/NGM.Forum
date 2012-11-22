using NGM.Forum.Models;

namespace NGM.Forum.Services.Popularity {
    public class DefaultPopularityService : IPopularityService {
        public double Calculate(ThreadPart thread) {
            return 0d;
        }

        public string Name {
            get { return "Default"; }
        }
    }
}