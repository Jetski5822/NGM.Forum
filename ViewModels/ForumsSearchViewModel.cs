namespace NGM.Forum.ViewModels {

    public class ForumsSearchViewModel {
        public string Query { get; set; }
        public int? ForumsHomeId { get; set; }
        public int TotalItemCount { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public dynamic ContentItems { get; set; }
        public dynamic Pager { get; set; }
        public dynamic ForumSearch { get; set; }
        public dynamic ForumMenu { get; set; }
        public dynamic BreadCrumb { get; set; }
    }

}