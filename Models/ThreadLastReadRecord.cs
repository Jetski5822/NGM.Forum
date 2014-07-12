using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.Models
{
    public class ThreadLastReadRecord
    {
        public virtual int Id { get; set; }
        public virtual int ThreadId { get; set; }
        public virtual int UserId { get; set; }
        public virtual DateTime? LastReadDate { get; set; } //used for sorting only
    }
}