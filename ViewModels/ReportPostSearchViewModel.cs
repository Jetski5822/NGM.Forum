using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.ViewModels
{
    public class ReportPostSearchViewModel
    {
        public ReportPostSearchViewModel()
        {
            //set as default
            this.ShowUnresolvedOnly = true;
        }
        public int? PostId { get; set; } //allows filtering on ids, since one post may be reported multiple times
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public String ReportedByUserName { get; set; }
        public String ReviewedByUserName { get; set; }
        public String ReportedUserName { get; set; }
        public Boolean ShowUnresolvedOnly { get; set; }        
        public string SearchButton { get; set; }

        public IEnumerable<ReportedPostEntry> SearchResults { get; set; }
        public dynamic Pager { get; set; }
     //   public ReportPostBulkAction BulkAction { get; set; }
    }
/*     
    public enum ReportPostBulkAction{
        MarkResolved
    }
 * */
}