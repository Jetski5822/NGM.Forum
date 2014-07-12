using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;
using System;

namespace NGM.Forum.Models {
    public class PostPart : ContentPart<PostPartRecord> {
        public int? RepliedOn {
            get { return Record.RepliedOn; }
            set { Record.RepliedOn = value; }
        }

        public string Text {
            get { return Record.Text; }
            set { Record.Text = value; }
        }

        public string Format {
            get { return Record.Format; }
            set { Record.Format = value; }
        }

        public DateTime? LastEdited
        {
            get { return Record.LastEdited; }
            set { Record.LastEdited = value; }
        }

        public ThreadPart ThreadPart {
            get { return this.As<ICommonPart>().Container.As<ThreadPart>(); }
            set { this.As<ICommonPart>().Container = value; }
        }

        public string IP
        {
            get { return Record.IP; }
            set { Record.IP = value; }
        }

        public bool IsParentThread() {
            return RepliedOn == null;
        }

        public bool IsInappropriate
        {
            get { return Record.IsInappropriate; }
            set { Record.IsInappropriate = value; }
        }

        //reflects the date of first post
        public DateTime PostedDate { get; set; }


        public string UserTimeZone
        {
            get;
            set;
        }

        public string UserCulture
        {
            get;
            set;
        }
    }

    public class PostPartRecord : ContentPartRecord {
        public virtual int? RepliedOn { get; set; }

        [StringLengthMax]
        public virtual string Text { get; set; }

        public virtual string Format { get; set; }

        public virtual DateTime? LastEdited { get; set; }

        public virtual bool IsInappropriate { get; set; }

        public virtual string IP { get; set; }
    }
}