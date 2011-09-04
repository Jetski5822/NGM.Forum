using System;

namespace NGM.Forum.Models {
    public class RedirectRouteRecord {
        public virtual int Id { get; set; }
        public virtual int ContentItemId { get; set; }
        public virtual string PreviousContainerSlug { get; set; }
        public virtual string PreviousSlug { get; set; }
        public virtual DateTime Expires { get; set; }
    }
}