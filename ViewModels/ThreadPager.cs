using System;
using Orchard.Settings;
using Orchard.UI.Navigation;

namespace NGM.Forum.ViewModels {
    public class ThreadPager : Pager {
        public ThreadPager(ISite site, int itemCount) :
            this(site, (int) Math.Ceiling((decimal) itemCount/(decimal) site.PageSize), site.PageSize) {
        }

        public ThreadPager(ISite site, PagerParameters pagerParameters)
            : base(site, pagerParameters) {
        }

        public ThreadPager(ISite site, int? page, int? pageSize)
            : base(site, page, pageSize) {
        }
    }
}