using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NGM.Forum.Models
{
    public class SubscriptionEmailTemplateRecord
    {
        public virtual int Id { get; set; }

        [Required]
        [StringLength(1024)]
        public virtual String Title { get; set; }

        [Required]
        [StringLengthMax]
        public virtual String BodyPlainText { get; set; }

        [Required]
        [StringLengthMax]
        public virtual String BodyHtml { get; set; }
    }
}