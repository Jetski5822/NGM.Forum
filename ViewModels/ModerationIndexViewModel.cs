using System.Collections.Generic;
using NGM.Forum.Models;

namespace NGM.Forum.ViewModels {

    public class ModerationIndexViewModel {
        public IList<ModerationEntry> Items { get; set; }
        public dynamic Pager { get; set; }
    }

    public class ModerationEntry {
        public ModerationPart Item { get; set; }
        public dynamic Shape { get; set; }
        public bool IsChecked { get; set; }
    }
}
