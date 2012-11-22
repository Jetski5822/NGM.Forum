using NGM.Forum.Models;
using Orchard;

namespace NGM.Forum.Services.Popularity {
    public interface IPopularityService : IDependency {
        double Calculate(ThreadPart thread);
        string Name { get; }
    }
}