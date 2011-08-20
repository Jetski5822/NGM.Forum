using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Routable.Models;
using Orchard.Data.Conventions;

namespace NGM.Forum.Models {
    public class ThreadPart : ContentPart<ThreadPartRecord> {
        [CascadeAllDeleteOrphan]
        public ForumPart ForumPart {
            get { return this.As<ICommonPart>().Container.As<ForumPart>(); }
            set { this.As<ICommonPart>().Container = value; }
        }

        public string Title {
            get { return this.As<RoutePart>().Title; }
            set { this.As<RoutePart>().Title = value; }
        }

        public int PostCount {
            get { return Record.PostCount; }
            set { Record.PostCount = value; }
        }

        public bool IsSticky {
            get { return Record.IsSticky; }
            set { Record.IsSticky = value; }
        }

        public bool IsClosed {
            get { return Record.IsClosed; }
            set { Record.IsClosed = value; }
        }

        public ThreadType Type {
            get { return (ThreadType)Record.Type; }
            set { Record.Type = (int)value; }
        }

        public bool IsAnswered {
            get { return Record.IsAnswered; }
            set { Record.IsAnswered = value; }            
        }

        public int NumberOfViews {
            get { return Record.NumberOfViews; }
            set { Record.NumberOfViews = value; }
        }

        public int ReplyCount {
            get { return PostCount >= 1 ? PostCount - 1 : 0; }
        }
    }
}