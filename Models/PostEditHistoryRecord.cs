using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.Models
{
    /* Implementing this as a repository.  Versioning probably would have worked as well
     * but don't need all its functionality so in theory using a repository should give the same
     * result with a little less overhead (?).  Have some doubts about which approach would be better.
     * 
     * The history will only be viewed by admins so its only loaded when required.
     */
    public class PostEditHistoryRecord
    {
        public virtual int Id { get; set; }
        public virtual int PostId { get; set; }
        public virtual int UserId { get; set; } //who edited the post


        public virtual String Text { get; set; } //who edited the post
        public virtual DateTime? EditDate { get; set; }
        public virtual String Format { get; set; }
    }
}