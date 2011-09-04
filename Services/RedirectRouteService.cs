using System;
using System.Collections.Generic;
using NGM.Forum.Models;
using Orchard;
using Orchard.ContentManagement.Aspects;
using Orchard.Data;

namespace NGM.Forum.Services {
    public interface IRedirectRouteService : IDependency {
        void CreateRedirect(IRoutableAspect containerRoutableAspect, IRoutableAspect childRoutableAspect, DateTime expires);
        IEnumerable<RedirectRouteRecord> Get(IRoutableAspect containerRoutableAspect, string childPath);
    }

    public class RedirectRouteService : IRedirectRouteService {
        private readonly IRepository<RedirectRouteRecord> _repository;

        public RedirectRouteService(IRepository<RedirectRouteRecord> repository) {
            _repository = repository;
        }

        public void CreateRedirect(IRoutableAspect containerRoutableAspect, IRoutableAspect childRoutableAspect, DateTime expires) {
            _repository.Create(new RedirectRouteRecord {
                ContentItemId = childRoutableAspect.Id,
                PreviousContainerSlug = containerRoutableAspect.Slug,
                PreviousSlug = childRoutableAspect.Slug,
                Expires = DateTime.MaxValue,
            });
        }

        public IEnumerable<RedirectRouteRecord> Get(IRoutableAspect containerRoutableAspect, string childPath) {
            return _repository.Fetch(o => o.PreviousContainerSlug == containerRoutableAspect.Slug && o.PreviousSlug == childPath);
        }
    }
}