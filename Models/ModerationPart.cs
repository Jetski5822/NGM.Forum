using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace NGM.Forum.Models {
    public class ModerationPart : ContentPart<ModerationPartRecord> {
        public bool Approved {
            get { return Record.Approved; }
            set { Record.Approved = value; }
        }

        public DateTime? ApprovalUtc {
            get { return Record.ApprovalUtc; }
            set { Record.ApprovalUtc = value; }
        }
    }

    public class ModerationPartRecord : ContentPartRecord {
        public virtual bool Approved { get; set; }

        public virtual DateTime? ApprovalUtc { get; set; }

        //[StringLengthMax]
        //public virtual string Text { get; set; }
    }
}