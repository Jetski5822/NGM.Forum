using System;
using NGM.Forum.Extensions;
using NGM.Forum.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace NGM.Forum.Models {

    public class ReportedPostRecord
    {
        public virtual int Id { get; set; }
        public virtual int PostId {get;set;}
        public virtual int PostedByUserId {get;set;}
        public virtual int ReportedByUserId {get;set;}
        public virtual DateTime? ReportedDate { get; set; }
        public virtual bool IsResolved {get;set;}
        public virtual DateTime? ResolvedDate{ get; set; }
        public virtual int ResolvedByUserId {get;set;}
        [StringLengthMax]
        public virtual String Note {get;set;}

        public virtual String ReasonReported { get; set; }

    }


}