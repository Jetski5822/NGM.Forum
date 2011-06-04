using NGM.Forum.Models;

namespace NGM.Forum.ViewModels {
    public class PostEditorViewModel {
        public PostPart PostPart { get; set; }

        public string Text {
            get { return PostPart.Text; }
            set { PostPart.Text = value; }
        }
    }
}