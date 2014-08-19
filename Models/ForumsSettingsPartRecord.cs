using System;
using NGM.Forum.Extensions;
using NGM.Forum.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace NGM.Forum.Models {

    public class ForumsSettingsPart : ContentPart<ForumsSettingsPartRecord>
    {

        public int ForumsHomeTitleMaximumLength
        {
            get { return this.Record.ForumsHomeTitleMaximumLength; }
            set { this.Record.ForumsHomeTitleMaximumLength = value; }
        }
        public int ForumsHomeUrlMaximumLength
        {
            get { return this.Record.ForumsHomeUrlMaximumLength; }
            set { this.Record.ForumsHomeUrlMaximumLength = value; }
        }

        public int CategoryTitleMaximumLength
        {
            get { return this.Record.CategoryTitleMaximumLength; }
            set { this.Record.CategoryTitleMaximumLength = value; }
        }
        public int CategoryUrlMaximumLength
        {
            get { return this.Record.CategoryUrlMaximumLength; }
            set { this.Record.CategoryUrlMaximumLength = value; }
        }

        public int ThreadTitleMaximumLength
        {
            get { return this.Record.ThreadTitleMaximumLength; }
            set { this.Record.ThreadTitleMaximumLength = value; }
        }
        public int ThreadUrlMaximumLength
        {
            get { return this.Record.ThreadUrlMaximumLength; }
            set { this.Record.ThreadUrlMaximumLength = value; }
        }

        public int DaysUntilThreadReadByDefault
        {
            get { return this.Record.DaysUntilThreadReadByDefault; }
            set { this.Record.DaysUntilThreadReadByDefault = value; }
        }

    }


    public class ForumsSettingsPartRecord : ContentPartRecord
    {

        public ForumsSettingsPartRecord()
        {
            //set some defaults if not already set
            if (this.ThreadTitleMaximumLength == 0) this.ThreadTitleMaximumLength = 200;
            if (this.ForumsHomeTitleMaximumLength == 0) this.ForumsHomeTitleMaximumLength = 30;
            if (this.CategoryTitleMaximumLength == 0) this.CategoryTitleMaximumLength = 30;
        }

        public virtual int ForumsHomeTitleMaximumLength { get; set; }
        public virtual int ForumsHomeUrlMaximumLength { get; set; }
        public virtual int CategoryTitleMaximumLength {get;set;}
        public virtual int CategoryUrlMaximumLength { get; set; }
        public virtual int ThreadTitleMaximumLength {get;set;}
        public virtual int ThreadUrlMaximumLength { get; set; }
        public virtual int DaysUntilThreadReadByDefault { get; set; }
    }

}