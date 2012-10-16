//using Orchard.ContentManagement;
//using Orchard.Data.Conventions;

//namespace NGM.Forum.Models {
//    public class ModerationPart : ContentPart<ModerationPartRecord> {
//        public bool Approved {
//            get { return Record.Approved; }
//            set { Record.Approved = value; }
//        }
//    }

//    public class ModerationPartRecord {
//        public virtual bool Approved { get; set; }

//        [StringLengthMax]
//        public virtual string Text { get; set; }
//    }
//}