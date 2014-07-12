using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.ViewModels
{
    public class PostEditHistoryEntry
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public String EditedBy { get; set; } //who edited the post
        public String Text { get; set; } //who edited the post
        public DateTime? EditDate { get; set; }
        public String Format { get; set; }
    }
}