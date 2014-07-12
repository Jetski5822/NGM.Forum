using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Title.Models;
using System;
using System.Collections.Generic;

namespace NGM.Forum.Models {

    public class ForumCategoryPart : ContentPart<ForumCategoryPartRecord>
    {
        public virtual List<ForumPart> Forums { get; set; }

        public string Title
        {
            get { return this.As<ITitleAspect>().Title; }
            set { this.As<TitlePart>().Title = value; }
        }

        public String Description {
            get { return Record.Description; }
            set { Record.Description = value; }
        }
        public int Weight
        {
            get { return Record.Weight; }
            set { Record.Weight = value; }
        }

        public ForumsHomePagePart ForumsHomePagePart
        {
            get { return this.As<ICommonPart>().Container.As<ForumsHomePagePart>(); }
            set { this.As<ICommonPart>().Container = value; }
        }

        public ForumCategoryPart()
        {
            Forums = new List<ForumPart>();
        }

    }
}
