using NGM.Forum.Models;

namespace NGM.Forum.ViewModels {
    public class PostBodyEditorViewModel {
        public PostPart PostPart { get; set; }

        public string Text {
            get { return PostPart.Record.Text; }
            set { PostPart.Record.Text = value; }
        }

        public string Format {
            get { return PostPart.Record.Format; }
            set { PostPart.Record.Format = value; }
        }

        public string EditorFlavor { get; set; }
    }
}