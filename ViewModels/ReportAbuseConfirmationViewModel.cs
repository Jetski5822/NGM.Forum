using NGM.Forum.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NGM.Forum.ViewModels
{
    public class ReportInappropriatePostConfirmationViewModel
    {

        public int PostId { get; set; }

        [Required]
        public String ReasonReported { get; set; }
        public String ReturnUrl { get; set; }

        public ReportPostService.CreatePostResultEnum ReportSubmittedResult { get; set; }
    }
}