using Orchard.Data.Conventions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NGM.Forum.Models
{
    public class SubscriptionEmailTemplateTranslation
    {
        public string Culture { get; set; }

        public String Title { get; set; }

        public String BodyPlainText { get; set; }

        public String BodyHtml { get; set; }
    }
}