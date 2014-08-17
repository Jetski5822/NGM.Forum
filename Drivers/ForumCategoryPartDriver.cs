using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using NGM.Forum.Models;
using System.Linq;
using System.Collections.Generic;
using NGM.Forum.ViewModels;
using NGM.Forum.Services;
using Orchard.Core.Title.Models;
using System;

namespace NGM.Forum.Drivers
{
    [UsedImplicitly]
	
    public class ForumCategoryPartDriver : ContentPartDriver<ForumCategoryPart>
    {
        private readonly INotifier _notifier;
        private readonly IContentManager _contentManager;
        private readonly IForumCategoryService _forumCategoryService;
        private readonly IForumService _forumService;
        private const string TemplateName = "Parts/ForumCategoryPart";
        
        public Localizer T { get; set; }

        public ForumCategoryPartDriver(
            INotifier notifier,
            IContentManager contentManager,
            IForumCategoryService forumCategoryService,
            IForumService forumService
        )
        {
            _notifier = notifier;
            T = NullLocalizer.Instance;
            _contentManager = contentManager;
            _forumCategoryService = forumCategoryService;
            _forumService = forumService;
        }

        protected override DriverResult Display(ForumCategoryPart part, string displayType, dynamic shapeHelper)
        {
            var results = new List<DriverResult>();
            if (displayType.Equals("SummaryAdmin", StringComparison.OrdinalIgnoreCase))
            {
                //loading associated forums per category this is not very efficient but its used in the admin so its low frequency
                results.Add(ContentShape("Parts_ForumCategory_RelatedForums_SummaryAdmin",
                    () => shapeHelper.Parts_ForumCategory_RelatedForums_SummaryAdmin(
                            ForumCategoryPart: part,
                            Forums: _forumCategoryService.GetForumsForCategory( part, VersionOptions.AllVersions).ToList()
                        )
                ));
                results.Add(ContentShape("Parts_ForumCategory_Menu_SummaryAdmin",
                    () => shapeHelper.Parts_ForumCategory_Menu_SummaryAdmin()                 
                ));

            }
            else
            {


                results.Add(ContentShape("Parts_ForumCategory",
                    () => shapeHelper.Parts_ForumCategory(
                        Category: part.ContentItem,
                        //this has been optimized a bit .. the controller has already loaded and divided the forum parts between the categories
                        Forums: part.Forums.Select(forum => _contentManager.BuildDisplay(forum, "Summary")).ToList()
                )));
            }

            return Combined(results.ToArray());
        }

        protected override DriverResult Editor(ForumCategoryPart part, dynamic shapeHelper)
        {
            part.Forums = _forumCategoryService.GetForumsForCategory(part, VersionOptions.AllVersions).ToList();
            return ContentShape("Parts_ForumCategoryPart_Edit",
                    () => shapeHelper.EditorTemplate(
                        TemplateName: "Parts.ForumCategoryPart.Edit", 
                        Model: BuildEditorViewModel(part), 
                        Prefix: Prefix));
        }

        protected override DriverResult Editor(ForumCategoryPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            var model = new ForumCategoryViewModel();
            if (updater.TryUpdateModel(model, Prefix, null, null))
            {
                if (part.ContentItem.Id != 0)
                {
                    part.Description = model.Description;
                    part.Weight = model.Weight;
                    //not sure if this would be better as a list of only published forums (i.e. what you see on the front end, or all forums i.e. drafts as well)
                    //in some ways this is not necessary but it will help administrators find the category with the forum they are looking for
                    part.Forums = _forumService.GetForumsForCategory( part, VersionOptions.Latest).ToList();

                    /*
                    if (model.ForumEntries != null)
                    {
                      //  _forumCategoryService.UpdateForumCategoryForumList(part.ContentItem, model.ForumEntries);
                       // var forumIds = model.ForumEntries.Where(e => e.IsChecked == true).Select(e => e.ForumPartRecord.Id).ToList();
                        part.Forums = _forumService.Get(VersionOptions.Latest).Where(f => forumIds.Contains(f.Id)).ToList();
                    }
                    else
                    {
                        part.Forums = new List<ForumPart>();
                    }
                     */

                    //not needed (?) already a success message shown by orchard
                    //_notifier.Error(T("The Forum Category was successfully updated"));
                }
            }
            else
            {
                _notifier.Error(T("An error occured while updating the Forum Category"));
            }

            return Editor(part, shapeHelper);
        }

        private ForumCategoryViewModel BuildEditorViewModel(ForumCategoryPart part)
        {
            //var selected = part.Forums.ToLookup(forum => forum.Id); 
            var viewModel = new ForumCategoryViewModel
            {
                 Title = part.Title,
                  Description = part.Description,
                  Weight = part.Weight,
                 ForumEntries = _forumService.Get(VersionOptions.Latest).Select( r => new ForumEntry
                {                      
                     Title = r.As<TitlePart>().Title,
                     ForumPartRecord = r.Record,
                     //IsChecked = selected.Contains(r.Id)
                }).ToList()
            };
            return viewModel;

        }

    }
}